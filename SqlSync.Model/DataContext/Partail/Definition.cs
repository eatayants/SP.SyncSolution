// ReSharper disable CheckNamespace
namespace Roster.Model.DataContext
// ReSharper restore CheckNamespace
{
	public partial class Definition
	{
		public string DisplayName
		{
			get
			{
				return string.IsNullOrEmpty(Code) ? Name : string.Format("{0} - {1}", Code, Name);
			}
		}
	}
}