using AutoMapper;
using AiPlugin.Domain;
using Microsoft.OpenApi.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Plugin, OpenApiInfo>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.NameForHuman))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DescriptionForHuman))
            .ForMember(dest => dest.Contact, opt => opt.MapFrom(src => new OpenApiContact { Email = src.ContactEmail }));
            //.ForMember(dest => dest.License, opt => opt.MapFrom(src => new OpenApiLicense { Url = src.LegalInfoUrl }));

        CreateMap<Section, OpenApiOperation>()
            .ForMember(dest => dest.Summary, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Content));
    }
}
