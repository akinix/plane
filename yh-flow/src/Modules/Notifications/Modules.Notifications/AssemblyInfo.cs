using System.Runtime.CompilerServices;
using YH.Framework.Web.Modules;

[assembly: FshModule(typeof(YH.Modules.Notifications.NotificationsModule), 750)]
[assembly: InternalsVisibleTo("Notifications.Tests")]
[assembly: InternalsVisibleTo("Integration.Tests")]
