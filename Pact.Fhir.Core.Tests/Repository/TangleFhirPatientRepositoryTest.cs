﻿namespace Pact.Fhir.Core.Tests.Repository
{
  using System;

  using Hl7.Fhir.Model;

  using Microsoft.VisualStudio.TestTools.UnitTesting;

  using Pact.Fhir.Core.Repository;
  using Pact.Fhir.Core.Repository.MamStorage;
  using Pact.Fhir.Core.Serializer;
  using Pact.Fhir.Core.Tests.Helpers;

  using Tangle.Net.Entity;

  using Task = System.Threading.Tasks.Task;

  /// <summary>
  /// The tangle fhir patient repository test.
  /// </summary>
  [TestClass]
  public class TangleFhirPatientRepositoryTest
  {
    /// <summary>
    /// The test created patient can be read.
    /// </summary>
    /// <returns>
    /// The <see cref="System.Threading.Tasks.Task"/>.
    /// </returns>
    [TestMethod]
    public async Task TestCreatedResourceCanBeRead()
    {
      var iotaRepository = new InMemoryIotaRepository();
      var repository = new TangleFhirPatientRepository(iotaRepository, new FhirJsonTryteSerializer(), new MemoryCacheStatefulMam(), NetMode.Testnet);
      var response = await repository.CreateResourceAsync(PatientHelpers.GetPatient(), Seed.Random(), Seed.Random());

      var patient = await repository.GetResourceAsync<Patient>(response.Message.Root);

      Assert.IsTrue(response.Resource.IsExactly(patient));
    }

    /// <summary>
    /// The test create sets up data and persists patient data.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/>.
    /// </returns>
    [TestMethod]
    public async Task TestCreateSetsUpDataAndPersistsResourceData()
    {
      var iotaRepository = new InMemoryIotaRepository();
      var repository = new TangleFhirPatientRepository(iotaRepository, new FhirJsonTryteSerializer(), new MemoryCacheStatefulMam(), NetMode.Testnet);
      var response = await repository.CreateResourceAsync(PatientHelpers.GetPatient(), Seed.Random(), Seed.Random());

      Assert.AreEqual(1, iotaRepository.SentBundles.Count);
      Assert.AreEqual(response.Message.Root.Value, response.Resource.Id);
      Assert.IsNotNull(response.Channel);
      Assert.IsNotNull(response.Message);
    }

    /// <summary>
    /// The test get history returns all resources publicated on stream.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/>.
    /// </returns>
    [TestMethod]
    public async Task TestGetHistoryReturnsAllResourcesPublishedOnStream()
    {
      var iotaRepository = new InMemoryIotaRepository();
      var repository = new TangleFhirPatientRepository(iotaRepository, new FhirJsonTryteSerializer(), new MemoryCacheStatefulMam(), NetMode.Testnet);
      var seed = Seed.Random();
      var createResponse = await repository.CreateResourceAsync(PatientHelpers.GetPatient(), seed, seed);

      var resource = createResponse.Resource;
      resource.Gender = AdministrativeGender.Female;

      await repository.UpdateResourceAsync(resource, seed);

      var resources = await repository.GetHistory<Patient>(createResponse.Message.Root);

      Assert.AreEqual(2, resources.Count);
      Assert.AreEqual(AdministrativeGender.Male, resources[0].Gender);
      Assert.AreEqual(AdministrativeGender.Female, resources[1].Gender);
    }

    /// <summary>
    /// The test update resource adds new bundle and sets version id to next root.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/>.
    /// </returns>
    [TestMethod]
    public async Task TestUpdateResourceAddsNewBundleAndSetsVersionIdToNextRoot()
    {
      var iotaRepository = new InMemoryIotaRepository();
      var repository = new TangleFhirPatientRepository(iotaRepository, new FhirJsonTryteSerializer(), new MemoryCacheStatefulMam(), NetMode.Testnet);
      var seed = Seed.Random();
      var createResponse = await repository.CreateResourceAsync(PatientHelpers.GetPatient(), seed, seed);

      var resource = createResponse.Resource;
      resource.Gender = AdministrativeGender.Female;

      var updateResponse = await repository.UpdateResourceAsync(resource, seed);

      var patient = await repository.GetResourceAsync<Patient>(createResponse.Message.Root);

      Assert.AreEqual(AdministrativeGender.Female, patient.Gender);
      Assert.AreEqual(AdministrativeGender.Female, updateResponse.Resource.Gender);
      Assert.AreEqual(2, iotaRepository.SentBundles.Count);
    }

    /// <summary>
    /// The test cached subscription returns same message as last message.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/>.
    /// </returns>
    [TestMethod]
    public async Task TestCachedSubscriptionReturnsSameMessageAsLastMessage()
    {
      var iotaRepository = new InMemoryIotaRepository();
      var repository = new TangleFhirPatientRepository(iotaRepository, new FhirJsonTryteSerializer(), new MemoryCacheStatefulMam(), NetMode.Testnet);
      var seed = Seed.Random();
      var createResponse = await repository.CreateResourceAsync(PatientHelpers.GetPatient(), seed, seed);

      var patient = await repository.GetResourceAsync<Patient>(createResponse.Message.Root);
      var secondPatient = await repository.GetResourceAsync<Patient>(createResponse.Message.Root);

      Assert.IsTrue(patient.IsExactly(secondPatient));
    }

    /// <summary>
    /// The test cached subscription returns same message history.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/>.
    /// </returns>
    [TestMethod]
    public async Task TestCachedSubscriptionReturnsSameMessageHistory()
    {
      var iotaRepository = new InMemoryIotaRepository();
      var repository = new TangleFhirPatientRepository(iotaRepository, new FhirJsonTryteSerializer(), new MemoryCacheStatefulMam(), NetMode.Testnet);
      var seed = Seed.Random();
      var createResponse = await repository.CreateResourceAsync(PatientHelpers.GetPatient(), seed, seed);

      var resource = createResponse.Resource;
      resource.Gender = AdministrativeGender.Female;

      await repository.UpdateResourceAsync(resource, seed);

      var resources = await repository.GetHistory<Patient>(createResponse.Message.Root);

      Assert.AreEqual(resources.Count, (await repository.GetHistory<Patient>(createResponse.Message.Root)).Count);
    }

    /// <summary>
    /// The test exception is thrown on unkown channel while updating.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/>.
    /// </returns>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task TestExceptionIsThrownOnUnkownChannelWhileUpdating()
    {
      var repository = new TangleFhirPatientRepository(new InMemoryIotaRepository(), new FhirJsonTryteSerializer(), new MemoryCacheStatefulMam(), NetMode.Testnet);
      await repository.UpdateResourceAsync(PatientHelpers.GetPatient(), Seed.Random());
    }

    /// <summary>
    /// The test patient can be updated after channel addition.
    /// </summary>
    /// <returns>
    /// The <see cref="Task"/>.
    /// </returns>
    [TestMethod]
    public async Task TestPatientCanBeUpdatedAfterChannelAddition()
    {
      var iotaRepository = new InMemoryIotaRepository();
      var tempRepository = new TangleFhirPatientRepository(iotaRepository, new FhirJsonTryteSerializer(), new MemoryCacheStatefulMam(), NetMode.Testnet);
      var seed = Seed.Random();
      var createResponse = await tempRepository.CreateResourceAsync(PatientHelpers.GetPatient(), seed, seed);

      var repository = new TangleFhirPatientRepository(iotaRepository, new FhirJsonTryteSerializer(), new MemoryCacheStatefulMam(), NetMode.Testnet);

      try
      {
        await repository.UpdateResourceAsync(createResponse.Resource, seed);
      }
      catch (Exception e)
      {
        Assert.IsInstanceOfType(e, typeof(ArgumentException));
      }

      await repository.AddChannel(createResponse.Channel);

      var resource = createResponse.Resource;
      resource.Gender = AdministrativeGender.Female;

      var updateResponse = await repository.UpdateResourceAsync(resource, seed);
      Assert.AreEqual(AdministrativeGender.Female, updateResponse.Resource.Gender);

      var resources = await repository.GetHistory<Patient>(createResponse.Message.Root);
      Assert.AreEqual(3, resources.Count);
    }
  }
}