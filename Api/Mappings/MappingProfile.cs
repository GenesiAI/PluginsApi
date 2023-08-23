using AiPlugin.Api.Dto;
using AiPlugin.Domain.Common.Manifest;
using AiPlugin.Domain.Plugin;
using AutoMapper;
using Microsoft.OpenApi.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Plugin, OpenApiDocument>()
            .ForMember(dest => dest.Info, opt => opt.MapFrom(
                src => new OpenApiInfo
                {
                    Title = src.NameForHuman,
                    Description = src.DescriptionForHuman,
                    Contact = new OpenApiContact { Email = src.ContactEmail },
                    Version = "v1",
                    // License = new OpenApiLicense { Url = new Uri(src.LegalInfoUrl) }
                }
                )
            )
            .ForMember(dest => dest.Paths, opt => opt.Ignore())  // Ignore this property during mapping
            .ForMember(dest => dest.Servers, opt => opt.MapFrom(src => new List<OpenApiServer>
            {
                new OpenApiServer { Url = GetPluginUrl(src.Id) }
            }))
            .AfterMap((src, dest) =>
            {
                dest.Paths = new OpenApiPaths();

                foreach (var section in src.Sections ?? new List<Section>())
                {
                    dest.Paths.Add($"/{section.Name}", new OpenApiPathItem
                    {
                        Operations = new Dictionary<OperationType, OpenApiOperation>(new List<KeyValuePair<OperationType, OpenApiOperation>>
                        {
                            new KeyValuePair<OperationType, OpenApiOperation>(OperationType.Get, new OpenApiOperation
                            {
                                OperationId = section.Name,
                                Summary = section.Description,
                                Responses = new OpenApiResponses
                                {
                                    { "200", new OpenApiResponse { Description = "Success" } }
                                }
                            })
                        })
                    });
                }
            });
        //    .ForMember(dest => dest.Paths, opt => opt.MapFrom(src => src.Sections))
        //    .ForMember(dest => dest.Servers, opt => opt.MapFrom(src => new List<OpenApiServer>
        //    {
        //        new OpenApiServer { Url = GetPluginUrl(src.Id) }
        //    }));

        //CreateMap<Section, OpenApiPathItem>()
        //    .ConvertUsing(src => new OpenApiPathItem
        //    {
        //        Operations = new Dictionary<OperationType, OpenApiOperation>(
        //            new List<KeyValuePair<OperationType, OpenApiOperation>>
        //            {
        //                new KeyValuePair<OperationType, OpenApiOperation>(OperationType.Get, new OpenApiOperation
        //                {
        //                    OperationId = src.Name,
        //                    Summary = src.Description,
        //                    Responses = new OpenApiResponses
        //                    {
        //                        { "200", new OpenApiResponse { Description = "Success" } }
        //                    }
        //                })
        //            }
        //         )

        //    });
        // .ForMember(dest => dest.Operations, opt => opt.MapFrom(src => new OpenApiOperation[]
        // {
        //     // paths:
        //     //     /todos:
        //     //         get:
        //     //         operationId: getTodos
        //     //         summary: Get the list of todos
        //     //         responses:
        //     //             "200":
        //     //             description: OK
        //     //             content:
        //     //                 application/json:
        //     //                 schema:
        //     //                     $ref: '#/components/schemas/getTodosResponse'


        CreateMap<Plugin, AiPluginManifest>()
            .ForMember(dest => dest.SchemaVersion, opt => opt.MapFrom(src => "v1"))
            .ForMember(dest => dest.NameForHuman, opt => opt.MapFrom(src => src.NameForHuman))
            .ForMember(dest => dest.NameForModel, opt => opt.MapFrom(src => src.NameForModel))
            .ForMember(dest => dest.DescriptionForHuman, opt => opt.MapFrom(src => src.DescriptionForHuman))
            .ForMember(dest => dest.DescriptionForModel, opt => opt.MapFrom(src => src.DescriptionForModel))
            .ForMember(dest => dest.Auth, opt => opt.MapFrom(src => new Auth
            {
                Type = "none"
            }))
            .ForMember(dest => dest.Api, opt => opt.MapFrom(src => new Api
            {
                Type = "openapi",
                Url = GetPluginUrl(src.Id) + "/openapi.json"
            }))
            .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => src.LogoUrl))
            .ForMember(dest => dest.ContactEmail, opt => opt.MapFrom(src => src.ContactEmail))
            .ForMember(dest => dest.LegalInfoUrl, opt => opt.MapFrom(src => src.LegalInfoUrl));

        CreateMap<PluginCreateRequest, Plugin>();
        CreateMap<PluginUpdateRequest, Plugin>();
        CreateMap<SectionCreateRequest, Section>();
        CreateMap<SectionUpdateRequest, Section>();

        CreateMap<IEnumerable<Plugin>, PluginsResponse>()
            .ForMember(dest => dest.PluginsCount, opt => opt.MapFrom(src => src.Count()))
            .ForMember(dest => dest.Plugins, opt => opt.MapFrom(src => src));

    }

    private string GetBaseUrl()
    {
        return /*env.IsDevelopment() ? "localhost:7210" :*/ "genesi.ai";
    }
    public string GetPluginUrl(Guid pluginId)
    {
        return $$"""https://{{pluginId}}.{{GetBaseUrl()}}""";
    }
}