﻿namespace Pact.Fhir.Core.Usecase.ReadResource
{
  using System;
  using System.Threading.Tasks;

  using Hl7.Fhir.Model;

  using Pact.Fhir.Core.Exception;
  using Pact.Fhir.Core.Repository;

  /// <summary>
  /// see https://www.hl7.org/fhir/http.html#read
  /// </summary>
  public class ReadResourceInteractor : UsecaseInteractor<ReadResourceRequest, ResourceUsecaseResponse>
  {
    /// <inheritdoc />
    public ReadResourceInteractor(IFhirRepository repository)
      : base(repository)
    {
    }

    /// <inheritdoc />
    public override async Task<ResourceUsecaseResponse> ExecuteAsync(ReadResourceRequest request)
    {
      try
      {
        var resource = await this.Repository.ReadResourceAsync(request.ResourceId);
        if (resource.ResourceType.ToString() != request.ResourceType)
        {
          throw new ResourceNotFoundException(request.ResourceId);
        }

        return new ResourceUsecaseResponse { Code = ResponseCode.Success, Resource = resource };
      }
      catch (ResourceNotFoundException exception)
      {
        return new ResourceUsecaseResponse { Code = ResponseCode.ResourceNotFound, ExceptionMessage = exception.Message };
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