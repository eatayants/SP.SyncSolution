﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using SqlDynamic.Queries;

namespace SqlDynamic.Providers
{
	public class Provider : IProvider
	{
		private class TsqlProvider
		{
			private readonly StringBuilder builder;
			private readonly Dictionary<Type, Action<object>> mappings;
			private readonly Dictionary<Constant, int> constants;

			private readonly List<SqlParameter> parameters;
			private int idCounter;

			public TsqlProvider(BaseQuery query)
			{
				parameters = new List<SqlParameter>();
				constants = new Dictionary<Constant, int>();

				builder = new StringBuilder();

				mappings = new Dictionary<Type, Action<object>>() {
					// Clauses
					{ typeof(SelectClause), x => Expand((Clause)x) },
					{ typeof(WhereClause), x => Expand((Clause)x) },
					{ typeof(FromClause), x => Expand((FromClause)x) },
					{ typeof(GroupByClause), x => Expand((GroupByClause)x) },
					{ typeof(OrderByClause), x => Expand((OrderByClause)x) },
					{ typeof(Skip), x => Expand((Skip)x) },
					{ typeof(Take), x => Expand((Take)x) },

					{ typeof(Query), x => Expand((Query)x) },
					{ typeof(UnionQuery), x => Expand((UnionQuery)x) },
					{ typeof(Table), x => Expand((Table)x) },
					{ typeof(OrderByItem), x => Expand((OrderByItem)x) },
					
					{ typeof(SubQuery), x => Expand((SubQuery)x) },
					{ typeof(TableSource), x => Expand((TableSource)x) },
					{ typeof(DerivedTable), x => Expand((DerivedTable)x) },
					{ typeof(SelectList), x => Expand((SelectList)x) },
					{ typeof(Select), x => Expand((Select)x) },

					{ typeof(Field), x => Expand((Field)x) },
					{ typeof(Constant), x => Expand((Constant)x) },
					{ typeof(JoinList), x => Expand((JoinList)x) },
					{ typeof(Star), x => Expand((Star)x) },
					{ typeof(Concatenation), x => Expand((Concatenation)x) },
					{ typeof(Function), x => Expand((Function)x) },
					{ typeof(Count), x => Expand((Function)x) },
					{ typeof(Sum), x => Expand((Function)x) },
					{ typeof(Coalesce), x => Expand((Coalesce)x) },
					{ typeof(Cast), x => Expand((Cast)x) },
					{ typeof(LiteralValue), x => Expand((LiteralValue)x) },
					{ typeof(LiteralExpression), x => Expand((LiteralExpression)x) },
                    { typeof(RowNumber), x => Expand((RowNumber)x) },

					{ typeof(Like), x => Expand((Operator)x) },
					{ typeof(Equal), x => Expand((Operator)x) },
					{ typeof(NotEqual), x => Expand((Operator)x) },
					{ typeof(LessThan), x => Expand((Operator)x) },
					{ typeof(LessThanOrEqual), x => Expand((Operator)x) },
					{ typeof(GreaterThan), x => Expand((Operator)x) },
					{ typeof(GreaterThanOrEqual), x => Expand((Operator)x) },

					{ typeof(Conjunction), x => Expand((Conjunction)x) },
					{ typeof(Disjunction), x => Expand((Disjunction)x) },
					{ typeof(InValues), x => Expand((InValues)x) },
                    { typeof(BitwiseAnd), x => Expand((BitwiseAnd)x) },
					{ typeof(InSubQuery), x => Expand((InSubQuery)x) },
					{ typeof(Not), x => Expand((Not)x) },
					{ typeof(IsNull), x => Expand((IsNull)x) },
					{ typeof(Between), x => Expand((Between)x) },
				};

				ExpandExpression(query);
			}

			private void ExpandExpression(object part)
			{
				if (part == null) {
					return;
				}

				var type = part.GetType();
				Action<object> expander;
				try {
					expander = mappings[type];
				} catch (Exception) {
					throw new Exception(string.Format("Could not expand {0}", type.Name));
				}
				expander.Invoke(part);
			}
			
			private void Expand(Query query)
			{
				ExpandExpression(query.SelectClause);
				ExpandExpression(query.FromClause);
				ExpandExpression(query.WhereClause);
				ExpandExpression(query.GroupByClause);
				ExpandExpression(query.OrderByClause);
				ExpandExpression(query.SkipClause);
				ExpandExpression(query.TakeClause);
			}

			private void Expand(UnionQuery unionQuery)
			{
				var first = true;
				foreach (var query in unionQuery.Queries) {
					if (first) {
						first = false;
					} else {
						builder.AppendLine().Append("union");
						if (unionQuery.UnionType == UnionType.All) {
							builder.Append(" all");
						}
						builder.AppendLine();
					}
					ExpandExpression(query);
				}
			}

			private void Expand(SubQuery subQuery)
			{
				builder.Append("(");
				ExpandExpression(subQuery.Query);
				builder.Append(")");
			}

			private void Expand(Clause clause)
			{
				var select = clause as SelectClause;

				if (!clause.ClauseList.IsEmpty) {
					if (select == null) {
						builder.AppendLine();
					}
					builder.Append(clause.ClauseName).Append(" ");

					if (select != null) {
						if (select.Distinct) {
							builder.Append("distinct ");
						}

						if (select.First != null) {
							builder.Append("top ").Append(select.First).Append(" ");
						}
					}
					
					ExpandExpression(clause.ClauseList);
				}
			}

			private void Expand(LiteralValue literalValue)
			{
				var value = literalValue.Value;

				if (value is int
				    || value is float
				    || value is double
				    || value is decimal) {

					builder.Append(value);
				} else {
					builder.Append('\'').Append(value).Append('\'');
				}
			}

			private void Expand(LiteralExpression literalExpression)
			{
				builder.Append(literalExpression.Expression);
			}

			//{ typeof(LiteralValue), x => Expand((LiteralValue)x) },
			//{ typeof(LiteralExpression), x => Expand((LiteralExpression)x) },

			private void Expand(FromClause clause)
			{
				builder.AppendLine();
				builder.Append(clause.ClauseName).Append(" ");
				ExpandExpression(clause.DataSource);

				if (!clause.ClauseList.IsEmpty) {
					builder.AppendLine();
					ExpandExpression(clause.ClauseList);
				}
			}

			// ReSharper disable once ParameterTypeCanBeEnumerable.Local
			private void Expand(JoinList joins)
			{
				var joinTypes = new Dictionary<JoinType, string>() {
					{ JoinType.Inner, "inner" },
					{ JoinType.LeftOuter, "left outer" },
					{ JoinType.RightOuter, "right outer" },
					{ JoinType.FullOuter, "full outer" },
					{ JoinType.Cross, "cross" },
				};

				var head = true;
				foreach (var join in joins) {
					if (head) { head = false; } else { builder.AppendLine(); }

					if (join.JoinType != JoinType.Inner) {
						builder.Append(joinTypes[join.JoinType]).Append(" ");
					}

					builder.Append("join ");
					ExpandExpression(join.Source);
					builder.Append(" on ");
					ExpandExpression(join.Expression);
				}
			}

			private void Expand(Table table)
			{
				builder.AppendFormat("[{0}]", table.Name);
			}

			private void Expand(TableSource tableSource)
			{
				ExpandExpression(tableSource.Table);

				if (!string.IsNullOrWhiteSpace(tableSource.Alias)) {
					builder.AppendFormat(" [{0}]", tableSource.Alias);
				}
			}

			private void Expand(DerivedTable derivedTable)
			{
				builder.AppendFormat("(");
				ExpandExpression(derivedTable.Query);
				builder.AppendFormat(")");
				builder.AppendFormat(" [{0}]", derivedTable.Alias);
			}

			private void Expand(Field field)
			{
				if (field.TabularDataSource != null) 
                {
					var source = field.TabularDataSource.Alias ?? ((TableSource) field.TabularDataSource).Table.Name;
					if (field.Name == "*") {
						builder.AppendFormat("[{0}].*", source);
					} else {
					    if (String.IsNullOrWhiteSpace(field.XmlFunction))
					    {
					        builder.AppendFormat("[{0}].[{1}]", source, field.Name);
					    }
					    else
					    {
                            builder.AppendFormat("[{0}].[{1}].{2}", source, field.Name, field.XmlFunction);
					    }
					}
				} 
                else 
                {
                    if (String.IsNullOrWhiteSpace(field.XmlFunction))
                    {
                        builder.AppendFormat("[{0}]", field.Name);
                    }
                    else
                    {
                        builder.AppendFormat("[{0}].{1}", field.Name, field.XmlFunction);
                    }
				}
			}

			private void Expand(Operator op)
			{
				ExpandExpression(op.LeftExpression);
				builder.Append(' ').Append(op.OperatorSymbol).Append(' ');
				ExpandExpression(op.RightExpression);
			}

			private void Expand(Between between)
			{
				ExpandExpression(between.TestExpression);
				builder.Append(" between ");
				ExpandExpression(between.LeftExpression);
				builder.Append(" and ");
				ExpandExpression(between.RightExpression);
			}

            private void Expand(BitwiseAnd bitwiseAnd)
            {
                ExpandExpression(bitwiseAnd.Bitmask);
                builder.Append(" & ");
                ExpandExpression(bitwiseAnd.Expression);
                builder.Append(" > 0 ");
            }

			private void Expand(Star star)
			{
				builder.Append("*");
			}

			private void Expand(SelectList selectList)
			{
				var head = true;
				foreach (var selectExpression in selectList) {
					if (head) { head = false; } else { builder.Append(", "); }
					ExpandExpression(selectExpression);
				}
			}

			private void Expand(Constant constantExpression)
			{
				string prexif = "p";

				var id = 0;
				if (!constants.TryGetValue(constantExpression, out id)) {
					id = idCounter++;

					var paramName = prexif + id;
					var value = constantExpression.Value;
					parameters.Add(new SqlParameter(paramName, value ?? DBNull.Value));
					constants.Add(constantExpression, id);
				}

				builder.Append("@" + prexif + id);
			}

			private void Expand(Select selectExpression)
			{
				ExpandExpression(selectExpression.Expression);

				if (!string.IsNullOrWhiteSpace(selectExpression.Alias)) {
					builder.AppendFormat(" as {0}", selectExpression.Alias);
				}
			}

			private void Expand(Concatenation concatenation)
			{
				var head = true;
				foreach (var item in concatenation.Expressions) {
					if (head) { head = false; } else { builder.Append(" + "); }
					ExpandExpression(item);
				}
			}

			private void Expand(Conjunction conjunction)
			{
				var wrap = !conjunction.RootConjunction;

				if (conjunction.Any()) {
					if (wrap) {
						builder.Append('(');
					}
					var head = true;
					foreach (var item in conjunction) {
					    if (head)
					    {
					        head = false;
					    }
					    else
					    {
                            builder.AppendLine().Append((item is Disjunction) ? "or " : "and ");
                        }
						ExpandExpression(item);
					}
					if (wrap) {
						builder.Append(')');
					}
				}
			}

			private void Expand(Disjunction disjunction)
			{
				if (disjunction.Any()) {
					builder.Append('(');

					var head = true;
					foreach (var item in disjunction) {
					    if (head)
					    {
					        head = false;
					    }
					    else
					    {
                            builder.AppendLine().Append((item is Disjunction) ? "or " : "and ");
					    }
						ExpandExpression(item);
					}

					builder.Append(')');
				}
			}

			private void Expand(Coalesce coalesce)
			{
				builder.Append("coalesce (");

				var head = true;
				foreach (var item in coalesce) {
					if (head) { head = false; } else { builder.Append(", "); }
					ExpandExpression(item);
				}

				builder.Append(')');
			}

			private void Expand(GroupByClause groupByClause)
			{
				if (!groupByClause.IsEmpty) {
					builder.AppendLine();
					builder.Append("group by ");

					var head = true;
					foreach (var item in groupByClause) {
						if (head) { head = false; } else { builder.Append(", "); }
						ExpandExpression(item);
					}
				}
			}

			private void Expand(OrderByClause orderByClause)
			{
				if (!orderByClause.IsEmpty) {
					builder.AppendLine();
					builder.Append("order by ");

					var head = true;
					foreach (var item in orderByClause) {
						if (head) { head = false; } else { builder.Append(", "); }
						ExpandExpression(item);
					}
				}
			}

			private void Expand(OrderByItem orderByItem)
			{
				ExpandExpression(orderByItem.Expression);
				if (orderByItem.ExplicitOrder) {
					builder.Append(" ").Append(orderByItem.Order == Order.Ascending ? "asc" : "desc");
				}
			}


			private void Expand(Function function)
			{
				builder.Append(function.FunctionName);
				builder.Append("(");
				ExpandExpression(function.Expression);
				builder.Append(")");
			}

			private void Expand(Cast cast)
			{
				builder.Append("cast (");
				ExpandExpression(cast.Expression);
				builder
					.Append(" as ")
					.Append(cast.Type)
					.Append(")");
			}

			private void Expand(Not not)
			{
				var isNull = not.Expression as IsNull;
				if (isNull == null) {
					builder.Append("not (");
					ExpandExpression(not.Expression);
					builder.Append(")");
				} else {
					ExpandExpression(isNull.Expression);
					builder.Append(" is not null");
				}
			}

			private void Expand(IsNull isNull)
			{
				ExpandExpression(isNull.Expression);
				builder.Append(" is null");
			}

            private void Expand(RowNumber rowNumber)
            {
                builder.Append("row_number() over(");
                if (rowNumber.UsePartitionBy)
                {
                    builder.Append("partition by ");
                    foreach (var item in rowNumber.PartitionBy)
                    {
                        ExpandExpression(item);
                    }
                    builder.Append(" ");
                }
                ExpandExpression(rowNumber.RowNumberOrderBy);
                builder.Append(")");
            }

			private void Expand(InValues inValues)
			{
				ExpandExpression(inValues.Expression);
				builder.Append(" in (");

				var head = true;
				foreach (var item in inValues.Values) {
					if (head) { head = false; } else { builder.Append(", "); }
					ExpandExpression(Expression.Const(item));
				}

				builder.Append(")");
			}

			private void Expand(InSubQuery inSubQuery)
			{
				ExpandExpression(inSubQuery.Expression);
				builder.Append(" in ");
				ExpandExpression(inSubQuery.SubQuery);
			}

			private void Expand(Skip skip)
			{
				builder.AppendLine().Append("offset ");
				ExpandExpression(new Constant(skip.Rows));
				builder.Append(" rows");
			}

			private void Expand(Take take)
			{
				builder.AppendLine().Append("fetch next ");
				ExpandExpression(new Constant(take.Rows)); // Could be TabularDataSource
				builder.Append(" rows only");
			}

			public IDbCommand GetCommand()
			{
				var command = new SqlCommand() {
					CommandText = GetSqlString(),
					CommandType = CommandType.Text,
				};

				command.Parameters.AddRange(parameters.ToArray());
				return command;
			}

			public string GetSqlString()
			{
				return builder.ToString();
			}
		}


		public IDbCommand GetCommand(BaseQuery query)
		{
			return new TsqlProvider(query).GetCommand();
		}

		public static IDbCommand GetCommand(Query query)
		{
			return new TsqlProvider(query).GetCommand();
		}

		string IProvider.GetSqlString(BaseQuery query)
		{
			return new TsqlProvider(query).GetSqlString();
		}

		public static string GetSqlString(BaseQuery query)
		{
			return new TsqlProvider(query).GetSqlString();
		}

	}
}
