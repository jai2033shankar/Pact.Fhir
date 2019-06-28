﻿namespace Pact.Fhir.Iota.Services
{
  using System;
  using System.Threading.Tasks;

  using Pact.Fhir.Iota.Entity;
  using Pact.Fhir.Iota.Events;
  using Pact.Fhir.Iota.Repository;

  using Tangle.Net.Cryptography;
  using Tangle.Net.Cryptography.Signing;
  using Tangle.Net.Entity;
  using Tangle.Net.Mam.Entity;
  using Tangle.Net.Mam.Merkle;
  using Tangle.Net.Mam.Services;
  using Tangle.Net.Repository;

  public abstract class DeterministicCredentialProvider : IChannelCredentialProvider
  {
    private const int ChannelKeyIndex = 1;

    private const int ChannelSeedIndex = 0;

    protected DeterministicCredentialProvider(
      IResourceTracker resourceTracker,
      ISigningHelper signingHelper,
      IAddressGenerator addressGenerator,
      IIotaRepository repository)
    {
      this.ResourceTracker = resourceTracker;
      this.SigningHelper = signingHelper;
      this.AddressGenerator = addressGenerator;
      this.SubscriptionFactory = new MamChannelSubscriptionFactory(repository, CurlMamParser.Default, CurlMask.Default);
    }

    public event EventHandler<SubscriptionEventArgs> SubscriptionFound;

    private IAddressGenerator AddressGenerator { get; }

    private IResourceTracker ResourceTracker { get; }

    private ISigningHelper SigningHelper { get; }

    private MamChannelSubscriptionFactory SubscriptionFactory { get; }

    /// <inheritdoc />
    public async Task<ChannelCredentials> CreateAsync(Seed seed)
    {
      // Create new channel credentials with the current index incremented by one
      return await this.FindAndUpdateCurrentIndexAsync(seed, await this.GetCurrentSubSeedIndexAsync(seed) + 1);
    }

    /// <inheritdoc />
    public async Task SyncAsync(Seed seed)
    {
      // Start sync with seed at index 0 (lowest index possible)
      await this.FindAndUpdateCurrentIndexAsync(seed, 1);
    }

    protected abstract Task<int> GetCurrentSubSeedIndexAsync(Seed seed);

    protected abstract Task SetCurrentSubSeedIndexAsync(Seed seed, int index);

    private async Task<ChannelCredentials> FindAndUpdateCurrentIndexAsync(Seed seed, int index)
    {
      while (true)
      {
        var subSeed = new Seed(Converter.TritsToTrytes(this.SigningHelper.GetSubseed(seed, index)));
        var channelSeed = new Seed((await this.AddressGenerator.GetAddressAsync(subSeed, SecurityLevel.Low, ChannelSeedIndex)).Value);
        var channelKey = (await this.AddressGenerator.GetAddressAsync(subSeed, SecurityLevel.Low, ChannelKeyIndex)).Value;

        var rootHash = CurlMerkleTreeFactory.Default.Create(channelSeed, 0, 1, IotaFhirRepository.SecurityLevel).Root.Hash;

        // Check if the index was used by another application. If not, return the corresponding channel credentials
        var subscription = this.SubscriptionFactory.Create(rootHash, Mode.Restricted, channelKey);
        var message = await subscription.FetchSingle(rootHash);
        if (message == null)
        {
          await this.SetCurrentSubSeedIndexAsync(seed, index);
          return new ChannelCredentials { Seed = channelSeed, ChannelKey = channelKey, RootHash = rootHash };
        }

        // The index is already in use. Increment by one and check that in the next round of the loop
        // Fire event for new subscription consumers can subscribe to
        this.SubscriptionFound?.Invoke(this, new SubscriptionEventArgs(subscription));
        index++;
      }
    }
  }
}