﻿namespace Pact.Fhir.Core.Tests.Usecase.UpdateResource
{
  using System;
  using System.Linq;

  using Hl7.Fhir.Model;
  using Hl7.Fhir.Serialization;

  using Microsoft.VisualStudio.TestTools.UnitTesting;

  using Moq;

  using Pact.Fhir.Core.Repository;
  using Pact.Fhir.Core.Tests.Repository;
  using Pact.Fhir.Core.Tests.Utils;
  using Pact.Fhir.Core.Usecase;
  using Pact.Fhir.Core.Usecase.UpdateResource;

  using Task = System.Threading.Tasks.Task;

  [TestClass]
  public class UpdateResourceInteractorTest
  {
    [DataRow("Patient", "{\"resourceType\":\"afafasf\"}", ResponseCode.UnprocessableEntity)]
    [DataTestMethod]
    public async Task TestFhirResourceCanNotBeDeserializedShouldReturnErrorCode(string type, string data, ResponseCode expectedCode)
    {

      var interactor = new UpdateResourceInteractor(new InMemoryFhirRepository(), new FhirJsonParser());
      var response = await interactor.ExecuteAsync(
                       new UpdateResourceRequest { ResourceJson = data });

      Assert.AreEqual(expectedCode, response.Code);
    }

    [TestMethod]
    public async Task TestIdsMismatchShouldReturnErrorCode()
    {
      var interactor = new UpdateResourceInteractor(new InMemoryFhirRepository(), new FhirJsonParser());
      var resource = FhirResourceProvider.Patient;
      resource.Id = "SOMEID";
      var response = await interactor.ExecuteAsync(
                       new UpdateResourceRequest { ResourceJson = new FhirJsonSerializer().SerializeToString(resource), ResourceId = "ANOTHERID" });

      Assert.AreEqual(ResponseCode.IdMismatch, response.Code);
    }

    [TestMethod]
    public async Task TestRepositoryThrowsExceptionShouldReturnErrorCode()
    {
      var repositoryMock = new Mock<IFhirRepository>();
      repositoryMock.Setup(r => r.UpdateResourceAsync(It.IsAny<DomainResource>())).ThrowsAsync(new Exception("Catch me if you can"));
      var interactor = new UpdateResourceInteractor(repositoryMock.Object, new FhirJsonParser());

      var resource = FhirResourceProvider.Patient;
      resource.Id = "SOMEID";

      var response = await interactor.ExecuteAsync(
                       new UpdateResourceRequest { ResourceJson = new FhirJsonSerializer().SerializeToString(resource), ResourceId = "SOMEID" });

      Assert.AreEqual(ResponseCode.Failure, response.Code);
    }

    [TestMethod]
    public async Task TestExistingResourceIsUpdated()
    {
      var inMemoryFhirRepository = new InMemoryFhirRepository();
      var interactor = new UpdateResourceInteractor(inMemoryFhirRepository, new FhirJsonParser());

      var createdResource = await inMemoryFhirRepository.CreateResourceAsync(FhirResourceProvider.Patient);

      await interactor.ExecuteAsync(
        new UpdateResourceRequest { ResourceJson = new FhirJsonSerializer().SerializeToString(createdResource), ResourceId = createdResource.Id });

      Assert.AreEqual(2, inMemoryFhirRepository.Resources.Count);
      Assert.AreEqual("SOMENEWVERSIONID", inMemoryFhirRepository.Resources.Last().Meta.VersionId);
    }
  }
}