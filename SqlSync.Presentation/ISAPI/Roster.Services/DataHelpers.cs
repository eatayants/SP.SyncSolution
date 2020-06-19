using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Roster.BL;
using Roster.Common;
using Roster.Presentation.Controls.Fields;

namespace Roster.Services
{
    public class DataHelpers
    {
        public string SubmitTimesheet(Guid listId, Guid itemId)
        {
            if (listId == Guid.Empty)
            {
                listId = TableIDs.TIMESHEET_ROSTERS;
            }
            var dataService = new RosterDataService();
            var roster = dataService.ListSingleRosterEvent(listId, itemId);
            var rosterProps = roster.RosterEventDictionary;
            if (rosterProps.ContainsKey(FieldNames.WORKER_PERSON_ID) && rosterProps[FieldNames.WORKER_PERSON_ID] != null)
            {
                // execute global Submit procedure
                return SubmitTimesheet(
                    "[dbo].[RorterEvents_TimesheetSubmit]",
                    rosterProps[FieldNames.WORKER_PERSON_ID].ToInt(),
                    itemId.ToSafeString(),
                    SPUtility.CreateISO8601DateTimeFromSystemDateTime(rosterProps[FieldNames.START_DATE].ToDateTime()),
                    SPUtility.CreateISO8601DateTimeFromSystemDateTime(rosterProps[FieldNames.END_DATE].ToDateTime())
                );
            }
            throw new Exception("Cannot find '" + FieldNames.WORKER_PERSON_ID + "' field in a roster or value of this field is NULL");
        }

        public string SubmitTimesheet(string procedureName, int workerId, string rosterIDs, string periodStart, string periodEnd)
        {
            var dataService = new RosterDataService();
            {
                string currentUserLogin = SPContext.Current.Web.CurrentUser.LoginName;
                List<Tuple<string, string>> whereCriteria = new List<Tuple<string, string>> {
                        new Tuple<string,string>(FieldNames.WORKER_AD_ACCOUNT, currentUserLogin)
                    };

                var currentWorkerId = dataService
                    .TableContent("WorkerPerson", "Id", "Id", whereCriteria)
                    .Select(x => { return x.Item1; }).FirstOrDefault();

                if (currentWorkerId == 0)
                    throw new Exception(string.Format("Cannot find '{0}' in Employee list", currentUserLogin));
                if (currentWorkerId != workerId)
                    throw new Exception("Current timesheet(s) can be submitted only by Worker with ID#" + workerId);

                List<Guid> ids = rosterIDs.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToGuid()).ToList();
                var rosters = dataService.GetRosterEvents(ids, true);
                // get unly rejected OR unconfirmed rosters
                var rostersInCorrectStatus = rosters.Where(r =>
                {
                    var _props = r.RosterEventDictionary;
                    return (_props.ContainsKey(FieldNames.STATUS_ID) &&
                            (_props[FieldNames.STATUS_ID].ToInt() == 6 || _props[FieldNames.STATUS_ID].ToInt() == 9));
                });

                var paramCollection = new List<Tuple<string, object>>();
                paramCollection.Add(new Tuple<string, object>("@startDate", periodStart));
                paramCollection.Add(new Tuple<string, object>("@endDate", periodEnd));
                paramCollection.Add(new Tuple<string, object>("@workerId", workerId));
                paramCollection.Add(new Tuple<string, object>("@workingRosterIds", string.Join(",", rostersInCorrectStatus.Where(r => r.EventTypeId == 1).Select(r => r.Id))));
                paramCollection.Add(new Tuple<string, object>("@timesheetRosterIds", string.Join(",", rostersInCorrectStatus.Where(r => r.EventTypeId == 3).Select(r => r.Id))));
                paramCollection.Add(new Tuple<string, object>("@currentUser", DbFieldUser.GetUserRosterId(SPContext.Current.Web.CurrentUser)));
                paramCollection.Add(new Tuple<string, object>("@message", string.Format("Sumbit at {0}", DateTime.Now.ToString(CultureInfo.InvariantCulture))));

                return dataService.ExecuteProcedure(procedureName, paramCollection);
            }
        }

        public string EndorseRoster(Guid listId, Guid itemId)
        {
            if (listId == Guid.Empty)
                listId = TableIDs.TIMESHEET_ROSTERS;

            var dataService = new RosterDataService();
            var roster = dataService.ListSingleRosterEvent(listId, itemId);
            var rosterProps = roster.RosterEventDictionary;
            if (rosterProps.ContainsKey(FieldNames.WORKER_PERSON_ID) && rosterProps[FieldNames.WORKER_PERSON_ID] != null)
            {
                if (!rosterProps.ContainsKey(FieldNames.STATUS_ID) || rosterProps[FieldNames.STATUS_ID] == null || rosterProps[FieldNames.STATUS_ID].ToInt() != 7)
                    throw new Exception("Only Confirmed roster can be endorsed!");

                int workerId = rosterProps[FieldNames.WORKER_PERSON_ID].ToInt();
                List<Tuple<string, string>> whereCriteria = new List<Tuple<string, string>> {
                        new Tuple<string,string>("Id", workerId.ToString())
                    };
                string currentUserLogin = SPContext.Current.Web.CurrentUser.LoginName;
                List<string> userLoginAndGroups = new List<string>();
                userLoginAndGroups.Add(currentUserLogin);
                userLoginAndGroups.AddRange(SPContext.Current.Web.CurrentUser.Groups.OfType<SPGroup>().Select(gr => gr.Name));

                var workerInfo = dataService.TableContent("WorkerPerson", "Id", FieldNames.WORKER_TEAMLEADER, whereCriteria).Select(x =>
                {
                    var elem = x.Item2 as IDictionary<string, object>;
                    return new { WorkerId = x.Item1, TeamLeader = elem[FieldNames.WORKER_TEAMLEADER].ToSafeString() };
                }).FirstOrDefault();

                if (workerInfo == null)
                    throw new Exception("Cannot find information about Worker with ID#" + workerId);
                //if (!workerInfo.TeamLeader.Equals(currentUserLogin))
                if (!userLoginAndGroups.Contains(workerInfo.TeamLeader))
                    throw new Exception("Only TeamLeader can endorse roster for selected Worker!");

                var paramCollection = new List<Tuple<string, object>>
                {
                    new Tuple<string, object>("@id", roster.Id.ToString()),
                    new Tuple<string, object>("@reason",
                        string.Format("Endorsed at {0} by {1}", DateTime.Now.ToString(), currentUserLogin)),
                    new Tuple<string, object>("@message", "")
                };
                return dataService.ExecuteProcedure("[dbo].[Endorsed]", paramCollection);
            }
            throw new Exception("Cannot find '" + FieldNames.WORKER_PERSON_ID + 
                "' field in a roster or value of this field is NULL!");
        }

        public string RejectRoster(Guid listId, Guid itemId, string reason)
        {
            if (listId == Guid.Empty) {
                listId = TableIDs.TIMESHEET_ROSTERS;
            }
            var dataService = new RosterDataService();
            var roster = dataService.ListSingleRosterEvent(listId,itemId);
            var rosterProps = roster.RosterEventDictionary;
            if (rosterProps.ContainsKey(FieldNames.WORKER_PERSON_ID) && rosterProps[FieldNames.WORKER_PERSON_ID] != null)
            {
                if (!rosterProps.ContainsKey(FieldNames.STATUS_ID) || rosterProps[FieldNames.STATUS_ID] == null ||
                    (rosterProps[FieldNames.STATUS_ID].ToInt() != 7 && rosterProps[FieldNames.STATUS_ID].ToInt() != 8))
                    throw new Exception("Only Confirmed/Endorsed roster can be rejected!");
                int statusId = rosterProps[FieldNames.STATUS_ID].ToInt();

                int workerId = rosterProps[FieldNames.WORKER_PERSON_ID].ToInt();
                List<Tuple<string, string>> whereCriteria = new List<Tuple<string, string>> {
                        new Tuple<string,string>("Id", workerId.ToString())
                    };

                string currentUserLogin = SPContext.Current.Web.CurrentUser.LoginName;
                List<string> userLoginAndGroups = new List<string>();
                userLoginAndGroups.Add(currentUserLogin);
                userLoginAndGroups.AddRange(SPContext.Current.Web.CurrentUser.Groups.OfType<SPGroup>().Select(gr => gr.Name));

                var workerInfo = dataService
                    .TableContent("WorkerPerson", "Id", FieldNames.WORKER_MANAGER + "$" + FieldNames.WORKER_TEAMLEADER, whereCriteria)
                    .Select(x =>
                    {
                        var elem = x.Item2 as IDictionary<string, object>;
                        return new { WorkerId = x.Item1, Manager = elem[FieldNames.WORKER_MANAGER].ToSafeString(), 
                            TeamLeader = elem[FieldNames.WORKER_TEAMLEADER].ToSafeString() };
                    })
                    .FirstOrDefault();

                if (workerInfo == null)
                    throw new Exception("Cannot find information about Worker with ID#" + workerId);
                //if (statusId == 7 && !workerInfo.TeamLeader.Equals(currentUserLogin))
                if (statusId == 7 && !userLoginAndGroups.Contains(workerInfo.TeamLeader))
                    throw new Exception("Only Team leader can reject Confirmed roster for selected worker!");
                if (statusId == 8 && !userLoginAndGroups.Contains(workerInfo.Manager))
                    throw new Exception("Only Manager can reject Endorsed roster for selected worker!");

                var paramCollection = new List<Tuple<string, object>>();
                paramCollection.Add(new Tuple<string, object>("@id", roster.Id.ToString()));
                paramCollection.Add(new Tuple<string, object>("@reason", reason));
                paramCollection.Add(new Tuple<string, object>("@message", ""));
                // execute stored prosedure
                return dataService.ExecuteProcedure("[dbo].[Rejected]", paramCollection);
            }
            throw new Exception("Cannot find '" + FieldNames.WORKER_PERSON_ID + 
                "' field in a roster or value of this field is NULL!");
        }

        public string ApproveRoster(Guid listId, Guid itemId)
        {
            if (listId == Guid.Empty) {
                listId = TableIDs.TIMESHEET_ROSTERS;              
            }
            var dataService = new RosterDataService();
            var roster = dataService.ListSingleRosterEvent(listId, itemId);
            var rosterProps = roster.RosterEventDictionary;
            if (rosterProps.ContainsKey(FieldNames.WORKER_PERSON_ID) && rosterProps[FieldNames.WORKER_PERSON_ID] != null)
            {
                if (!rosterProps.ContainsKey(FieldNames.STATUS_ID) ||
                    rosterProps[FieldNames.STATUS_ID] == null ||
                    rosterProps[FieldNames.STATUS_ID].ToInt() != 8)
                {
                    throw new Exception("Only Endorsed roster can be approved!");
                }
                var workerId = rosterProps[FieldNames.WORKER_PERSON_ID].ToInt();
                var whereCriteria = new List<Tuple<string, string>>  {
                    new Tuple<string,string>("Id", workerId.ToString())
                };
                var currentUserLogin = SPContext.Current.Web.CurrentUser.LoginName;
                List<string> userLoginAndGroups = new List<string>();
                userLoginAndGroups.Add(currentUserLogin);
                userLoginAndGroups.AddRange(SPContext.Current.Web.CurrentUser.Groups.OfType<SPGroup>().Select(gr => gr.Name));
                var workerInfo = dataService.TableContent("WorkerPerson", "Id", FieldNames.WORKER_MANAGER, whereCriteria).Select(x =>
                {
                    var elem = x.Item2 as IDictionary<string, object>;
                    return new { WorkerId = x.Item1, Manager = elem[FieldNames.WORKER_MANAGER].ToSafeString() };
                }).FirstOrDefault();

                if (workerInfo == null) {
                    throw new Exception("Cannot find information about Worker with ID#" + workerId);
                }
                //if (!workerInfo.Manager.Equals(currentUserLogin))
                if (!userLoginAndGroups.Contains(workerInfo.Manager))
                {
                    throw new Exception("Only Manager can approve roster for selected Worker!");
                }

                var paramCollection = new List<Tuple<string, object>>();
                paramCollection.Add(new Tuple<string, object>("@id", roster.Id.ToString()));
                paramCollection.Add(new Tuple<string, object>("@reason", ""));
                paramCollection.Add(new Tuple<string, object>("@message", ""));
                // execute stored prosedure
                return dataService.ExecuteProcedure("[dbo].[Approved]", paramCollection);
            }
            throw new Exception("Cannot find '" + FieldNames.WORKER_PERSON_ID + "' field in a roster or value of this field is NULL");
        }

        public string CancelRoster(Guid itemId)
        {
            var currentUserLogin = SPContext.Current.Web.CurrentUser.LoginName;
            var paramCollection = new List<Tuple<string, object>>
            {
                new Tuple<string, object>("@id", itemId.ToString()),
                new Tuple<string, object>("@reason",
                    string.Format("Canceled at {0} by {1}", DateTime.Now.ToString(), currentUserLogin)),
                new Tuple<string, object>("@message", "")
            };
            // execute stored prosedure
            return new RosterDataService().ExecuteProcedure("[dbo].[Cancelled]", paramCollection);
        }
    }
}
