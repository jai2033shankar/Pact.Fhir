﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Pact.Fhir.Core.Tests.Usecase.VersionReadResource
{
  using System.Threading.Tasks;

  using Microsoft.VisualStudio.TestTools.UnitTesting;

  using Moq;

  using Pact.Fhir.Core.Repository;
  using Pact.Fhir.Core.Tests.Repository;
  using Pact.Fhir.Core.Tests.Utils;
  using Pact.Fhir.Core.Usecase;
  using Pact.Fhir.Core.Usecase.ReadResource;
  using Pact.Fhir.Core.Usecase.VersionReadResource;

  using Exception = System.Exception;

  [TestClass]
  public class VersionReadResourceInteractorTest
  {
    [TestMethod]
    public async Task TestRepositoryThrowsExceptionShouldReturnErrorCode()
    {
      var repository = new Mock<IFhirRepository>();
      repository.Setup(r => r.ReadResourceAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

      var interactor = new VersionReadResourceInteractor(repository.Object);
      var response = await interactor.ExecuteAsync(
                       new VersionReadResourceRequest { ResourceId = "kasfasagdssg", VersionId = "aaaaaaaaa", ResourceType = "Patient" });

      Assert.AreEqual(ResponseCode.Failure, response.Code);
    }

    [TestMethod]
    public async Task TestResourceDoesNotExistShouldReturnErrorCode()
    {
      var interactor = new VersionReadResourceInteractor(new InMemoryFhirRepository());
      var response = await interactor.ExecuteAsync(
                       new VersionReadResourceRequest { ResourceId = "kasfasagdssg", VersionId = "aaaaaaaaa", ResourceType = "Patient" });

      Assert.AreEqual(ResponseCode.ResourceNotFound, response.Code);
    }

    [TestMethod]
    public async Task TestResourceExistsShouldReturnResourceAndSuccess()
    {
      var resource = FhirResourceProvider.Patient;
      resource.VersionId = "SOMEFHIRCONFORMID";

      var repository = new InMemoryFhirRepository();
      repository.Resources.Add(resource);

      var interactor = new VersionReadResourceInteractor(repository);
      var response = await interactor.ExecuteAsync(
                       new VersionReadResourceRequest { ResourceId = "kasfasagdssg", VersionId = "SOMEFHIRCONFORMID", ResourceType = "Patient" });

      Assert.AreEqual(ResponseCode.Success, response.Code);
      Assert.IsTrue(response.Resource.IsExactly(resource));
    }

    [TestMethod]
    public async Task TestResourceTypeMismatchShouldReturnErrorCode()
    {
      var resource = FhirResourceProvider.Patient;
      resource.Id = "SOMEFHIRCONFORMID";

      var repository = new InMemoryFhirRepository();
      repository.Resources.Add(resource);

      var interactor = new VersionReadResourceInteractor(repository);
      var response = await interactor.ExecuteAsync(
                       new VersionReadResourceRequest { ResourceId = "kasfasagdssg", VersionId = "aaaaaaaaa", ResourceType = "Patient" });

      Assert.AreEqual(ResponseCode.ResourceNotFound, response.Code);
    }
  }
}
