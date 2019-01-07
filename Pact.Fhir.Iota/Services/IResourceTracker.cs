﻿namespace Pact.Fhir.Iota.Services
{
  using Pact.Fhir.Iota.Entity;

  using Tangle.Net.Entity;

  /// <summary>
  /// Since the restricted MAM mode is used, we need to keep track of the channel keys belonging to certain roots
  /// </summary>
  public interface IResourceTracker
  {
    void AddEntry(ResourceEntry entry);

    TryteString GetChannelKey(Hash versionId);
  }
}