﻿namespace Pact.Fhir.Core.Usecase.CreateResource
{
  using System;
  using System.Threading.Tasks;

  using Hl7.Fhir.Model;
  using Hl7.Fhir.Serialization;

  using Pact.Fhir.Core.Repository;

  /// <summary>
  /// see http://hl7.org/fhir/http.html#create
  /// </summary>
  public class CreateResourceInteractor : UsecaseInteractor<CreateResourceRequest, ResourceUsecaseResponse>
  {
    /// <inheritdoc />
    public CreateResourceInteractor(IFhirRepository repository, FhirJsonParser fhirParser)
      : base(repository)
    {
      this.FhirParser = fhirParser;
    }

    public FhirJsonParser FhirParser { get; }

    public override async Task<ResourceUsecaseResponse> ExecuteAsync(CreateResourceRequest request)
    {
      try
      {
        var requestResource = this.FhirParser.Parse<DomainResource>(request.ResourceJson);
        var resource = await this.Repository.CreateResourceAsync(requestResource);

        return new ResourceUsecaseResponse { Code = ResponseCode.Success, Resource = resource };
      }
      catch (FormatException exception)
      {
        return new ResourceUsecaseResponse { Code = ResponseCode.UnprocessableEntity, ExceptionMessage = exception.Message };
      }
      catch (Exception)
      {
        return new ResourceUsecaseResponse
        {
                   Code = ResponseCode.Failure, ExceptionMessage = "Given resource was not processed. Please take a look at internal logs."
                 };
      }
    }
  }
}