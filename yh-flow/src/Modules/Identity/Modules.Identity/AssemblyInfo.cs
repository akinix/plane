using YH.Framework.Web.Modules;
using System.Runtime.CompilerServices;

[assembly: FshModule(typeof(YH.Modules.Identity.IdentityModule), 100)]
[assembly: InternalsVisibleTo("Identity.Tests")]