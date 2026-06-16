using System.Runtime.CompilerServices;
using YH.Framework.Web.Modules;

[assembly: FshModule(typeof(YH.Modules.Chat.ChatModule), 800)]
[assembly: InternalsVisibleTo("Chat.Tests")]
[assembly: InternalsVisibleTo("Integration.Tests")]
