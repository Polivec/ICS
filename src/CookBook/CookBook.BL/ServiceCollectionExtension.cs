﻿using CookBook.BL.Facades;
using CookBook.BL.Mappers;
using CookBook.BL.Models;
using CookBook.DAL.Entities;
using CookBook.DAL.Mappers;
using CookBook.DAL.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace CookBook.BL;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddBLServices(this IServiceCollection services)
    {
        services.AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();

        services.AddSingleton<IFacade<RecipeEntity, RecipeListModel, RecipeDetailModel, RecipeEntityMapper>, RecipeFacade>();
        services.AddSingleton<IIngredientAmountFacade, IngredientAmountFacade>();
        services.AddSingleton(typeof(IFacade<,,,>), typeof(Facade<,,,>));


        services.AddSingleton<IIngredientAmountModelMapper, IngredientAmountModelMapper>();

        services.Scan(selector => selector
            .FromAssemblyOf<BusinessLogic>()
            .AddClasses(filter => filter.AssignableTo(typeof(ModelMapperBase<,,>)))
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

        return services;
    }
}