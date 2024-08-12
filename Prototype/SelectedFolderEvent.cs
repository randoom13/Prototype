
using Prototype.Main.GraphSourceParts;

namespace Prototype.Main
{
    internal class SelectedFolderEvent : PubSubEvent<string> { }

    internal class BuildStatisticEvent : PubSubEvent<GraphInfos> { }
}
