﻿namespace Pact.Fhir.Core.Repository
{
  using System.Collections.Generic;
  using System.Threading.Tasks;

  using Hl7.Fhir.Model;

  using Pact.Fhir.Core.Repository.Responses;

  using Tangle.Net.Entity;
  using Tangle.Net.Mam.Entity;

  using Task = System.Threading.Tasks.Task;

  /// <summary>
  /// The FhirPatientRepository interface.
  /// </summary>
  public interface IFhirPatientRepository
  {
    /// <summary>
    /// The add channel.
    /// </summary>
    /// <param name="channel">
    /// The channel.
    /// </param>
    /// <returns>
    /// The <see cref="System.Threading.Tasks.Task"/>.
    /// </returns>
    Task AddChannel(MamChannel channel);

    /// <summary>
    /// The create.
    /// </summary>
    /// <param name="resource">
    /// The resource.
    /// </param>
    /// <param name="seed">
    /// The seed.
    /// </param>
    /// <param name="channelKey">
    /// The channel Key.
    /// </param>
    /// <typeparam name="T">
    /// The resource type.
    /// </typeparam>
    /// <returns>
    /// The <see cref="System.Threading.Tasks.Task"/>.
    /// </returns>
    Task<ResourceReponse<T>> CreateResourceAsync<T>(T resource, Seed seed, TryteString channelKey)
      where T : DomainResource;

    /// <summary>
    /// The get history.
    /// </summary>
    /// <param name="root">
    /// The root.
    /// </param>
    /// <typeparam name="T">
    /// The resource type.
    /// </typeparam>
    /// <returns>
    /// The <see cref="Task{TResult}"/>.
    /// </returns>
    Task<List<T>> GetHistory<T>(Hash root)
      where T : DomainResource;

    /// <summary>
    /// The get patient async.
    /// </summary>
    /// <typeparam name="T">
    /// The resource type.
    /// </typeparam>
    /// <param name="root">
    /// The root.
    /// </param>
    /// <returns>
    /// The <see cref="System.Threading.Tasks.Task"/>.
    /// </returns>
    Task<T> GetResourceAsync<T>(Hash root)
      where T : DomainResource;

    /// <summary>
    /// The get resource version.
    /// </summary>
    /// <param name="root">
    /// The root.
    /// </param>
    /// <typeparam name="T">
    /// The resource type.
    /// </typeparam>
    /// <returns>
    /// The <see cref="Task"/>.
    /// </returns>
    Task<T> GetResourceVersion<T>(Hash root)
      where T : DomainResource;

    /// <summary>
    /// The has channel.
    /// </summary>
    /// <param name="seed">
    /// The seed.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/>.
    /// </returns>
    Task<bool> HasChannel(Seed seed);

    /// <summary>
    /// The update resource async.
    /// </summary>
    /// <param name="resource">
    /// The resource.
    /// </param>
    /// <param name="seed">
    /// The seed.
    /// </param>
    /// <typeparam name="T">
    /// The resource type.
    /// </typeparam>
    /// <returns>
    /// The <see cref="Task{TResult}"/>.
    /// </returns>
    Task<ResourceReponse<T>> UpdateResourceAsync<T>(T resource, Seed seed)
      where T : DomainResource;
  }
}