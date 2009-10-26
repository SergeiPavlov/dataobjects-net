using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Disconnected
{
  [DebuggerDisplay("Count = {Items.Count}, IsFullyLoaded = {IsFullyLoaded}")]
  internal sealed class DisconnectedEntitySetState
  {
    private bool? isFullyLoaded;

    public DisconnectedEntitySetState Origin { get; private set; }

    public IDictionary<Key, Key> Items { get; private set; }

    public bool IsFullyLoaded
    {
      get
      {
        if (isFullyLoaded.HasValue)
          return isFullyLoaded.Value;
        if (Origin!=null)
          return Origin.IsFullyLoaded;
        return false;
      }
      set
      {
        isFullyLoaded = value;
      }
    }

    public void Commit()
    {
      var items = Items as ChainedDictionary<Key, Key>;
      if (items==null)
        return;
      items.Commit();
      if (isFullyLoaded.HasValue)
        Origin.IsFullyLoaded = isFullyLoaded.Value;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="items">The items.</param>
    /// <param name="isFullyLoaded">if set to <see langword="true"/> state is fully loaded.</param>
    public DisconnectedEntitySetState(IEnumerable<Key> items, bool isFullyLoaded)
    {
      Items = new Dictionary<Key, Key>();
      if (items!=null)
        foreach (var key in items)
          Items.Add(key, key);
      IsFullyLoaded = isFullyLoaded;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The origin state.</param>
    public DisconnectedEntitySetState(DisconnectedEntitySetState origin)
    {
      Origin = origin;
      Items = new ChainedDictionary<Key, Key>(origin.Items);
      IsFullyLoaded = origin.IsFullyLoaded;
    }
  }
}