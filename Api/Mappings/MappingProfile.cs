using AutoMapper;
using AiPlugin.Domain;
using Microsoft.OpenApi.Models;
using AiPlugin.Domain.Manifest;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // CreateMap<Plugin, OpenApiInfo>()
        //     .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.NameForHuman))
        //     .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DescriptionForHuman))
        //     .ForMember(dest => dest.Contact, opt => opt.MapFrom(src => new OpenApiContact { Email = src.ContactEmail }));
        // //.ForMember(dest => dest.License, opt => opt.MapFrom(src => new OpenApiLicense { Url = src.LegalInfoUrl }));

        CreateMap<Plugin, OpenApiDocument>()
            .ForMember(dest => dest.Info, opt => opt.MapFrom(src => new OpenApiInfo
            {
                Title = src.NameForHuman,
                Description = src.DescriptionForHuman,
                Contact = new OpenApiContact { Email = src.ContactEmail },
                // License = new OpenApiLicense { Url = src.LegalInfoUrl }
            }))
            .ForMember(dest => dest.Paths, opt => opt.MapFrom(src => new OpenApiPaths()));

        // CreateMap<Section, OpenApiOperation>()
        //     .ForMember(dest => dest.Summary, opt => opt.MapFrom(src => src.Name))
        //     .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Content));

        CreateMap<Plugin, AiPluginManifest>()
            .ForMember(dest => dest.NameForHuman, opt => opt.MapFrom(src => src.NameForHuman))
            .ForMember(dest => dest.DescriptionForHuman, opt => opt.MapFrom(src => src.DescriptionForHuman))
            .ForMember(dest => dest.ContactEmail, opt => opt.MapFrom(src => src.ContactEmail))
            .ForMember(dest => dest.LegalInfoUrl, opt => opt.MapFrom(src => src.LegalInfoUrl))
            // .ForMember(dest => dest.Api, opt => opt.MapFrom(src => src.Sections))
            ;
    }
}
