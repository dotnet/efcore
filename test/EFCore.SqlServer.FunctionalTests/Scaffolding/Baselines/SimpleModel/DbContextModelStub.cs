using Microsoft.EntityFrameworkCore.Metadata;

namespace TestNamespace
{
    public partial class DbContextModel
    {
		static IModel _model = null!;

		static partial void OnModelFinalized(IModel model)
			=> _model = model;

        public static IModel GetModel()
            => _model;
    }
}