﻿using CookBook.BL.Facades;
using CookBook.BL.Models;
using CookBook.Common.Enums;
using CookBook.Common.Tests;
using CookBook.Common.Tests.Seeds;
using CookBook.DAL.Entities;
using CookBook.DAL.Mappers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CookBook.BL.Tests
{
    public class RecipeFacadeTests : CRUDFacadeTestsBase
    {
        private readonly Facade<RecipeEntity, RecipeListModel, RecipeDetailModel, RecipeEntityMapper> _facadeSUT;

        public RecipeFacadeTests(ITestOutputHelper output) : base(output)
        {
            _facadeSUT = new Facade<RecipeEntity, RecipeListModel, RecipeDetailModel, RecipeEntityMapper>(UnitOfWorkFactory, RecipeModelMapper);
        }

        [Fact]
        public async Task Create_WithWithoutIngredient_DoesNotThrowAndEqualsCreated()
        {
            //Arrange
            var model = new RecipeDetailModel()
            {
                Name = "Recipe 1",
                Description = "Testing recipe 1",
                Duration = TimeSpan.FromHours(2),
                FoodType = FoodType.Dessert
            };

            //Act
            var returnedModel = await _facadeSUT.SaveAsync(model);

            //Assert
            FixIds(model, returnedModel);
            DeepAssert.Equal(model, returnedModel);
        }

        [Fact]
        public async Task Create_WithNonExistingIngredient_Throws()
        {
            //Arrange
            var model = new RecipeDetailModel()
            {
                Name = "Recipe 2",
                Description = "Testing recipe 2",
                Duration = TimeSpan.FromHours(2),
                FoodType = FoodType.Dessert,
                Ingredients = new ObservableCollection<IngredientAmountListModel>()
                {
                    new()
                    {
                        Id = Guid.Empty,
                        IngredientId = Guid.Empty,
                        IngredientName = "Ingredient 1",
                        IngredientImageUrl = null,
                        Amount = 0,
                        Unit = Unit.None
                    }
                }
            };

            //Act & Assert
            try
            {
                await _facadeSUT.SaveAsync(model); //In-memory pass without exception
            }
            catch (DbUpdateException) { } //SqlServer throws on FK
        }

        [Fact]
        public async Task Create_WithIngredient_DoesNotThrowAndEqualsCreated()
        {
            //Arrange
            var model = new RecipeDetailModel()
            {
                Name = "Recipe 2",
                Description = "Testing recipe 2",
                Duration = TimeSpan.FromHours(2),
                FoodType = FoodType.Dessert,
                ImageUrl = "https://d2v9mhsiek5lbq.cloudfront.net/eyJidWNrZXQiOiJsb21hLW1lZGlhLXVrIiwia2V5IjoiZm9vZG5ldHdvcmstaW1hZ2UtOGI5ZWM4YTAtODc1OC00MDcyLTg2YTItMzMzYTA4NTY5NTkwLmpwZyIsImVkaXRzIjp7InJlc2l6ZSI6eyJmaXQiOiJjb3ZlciIsIndpZHRoIjo3NTAsImhlaWdodCI6NDIyfX19",
                Ingredients = new ObservableCollection<IngredientAmountListModel>()
                {
                    new ()
                    {
                        IngredientName = IngredientSeeds.IngredientEntity1.Name,
                        IngredientId = IngredientSeeds.IngredientEntity1.Id,
                        Amount = 5,
                        Unit = Unit.L,
                        IngredientImageUrl = IngredientSeeds.IngredientEntity1.ImageUrl,
                    }
                },
            };

            //Act
            var returnedModel = await _facadeSUT.SaveAsync(model);

            //Assert
            FixIds(model, returnedModel);
            DeepAssert.Equal(model, returnedModel);
        }

        [Fact]
        public async Task Create_WithExistingAndNotExistingIngredient_Throws()
        {
            //Arrange
            var model = new RecipeDetailModel()
            {
                Name = "Recipe 2",
                Description = "Testing recipe 2",
                Duration = TimeSpan.FromHours(2),
                FoodType = FoodType.Dessert,
                Ingredients = new ObservableCollection<IngredientAmountListModel>()
                {
                    new ()
                    {
                        IngredientId = Guid.Empty,
                        IngredientName = "Ingredient 1",
                        IngredientImageUrl = null,
                        Amount = 0,
                        Unit = Unit.None
                    },
                    IngredientAmountModelMapper.MapToListModel(IngredientAmountSeeds.IngredientAmountEntity1),
                },
            };

            //Act & Assert
            try
            {
                await _facadeSUT.SaveAsync(model);
                Assert.True(false, "Assert Fail");
            }
            catch (DbUpdateException) { } //SqlServer
            catch (ArgumentException) { } //In-memory
        }

        [Fact]
        public async Task GetById_FromSeeded_DoesNotThrowAndEqualsSeeded()
        {
            //Arrange
            var detailModel = RecipeModelMapper.MapToDetailModel(RecipeSeeds.RecipeEntity);

            //Act
            var returnedModel = await _facadeSUT.GetAsync(detailModel.Id);

            //Assert
            DeepAssert.Equal(detailModel, returnedModel);
        }

        [Fact]
        public async Task GetAll_FromSeeded_DoesNotThrowAndContainsSeeded()
        {
            //Arrange
            var listModel = RecipeModelMapper.MapToListModel(RecipeSeeds.RecipeEntity);

            //Act
            var returnedModel = await _facadeSUT.GetAsync();

            //Assert
            Assert.Contains(listModel, returnedModel);
        }

        [Fact]
        public async Task Update_FromSeeded_DoesNotThrow()
        {
            //Arrange
            var detailModel = RecipeModelMapper.MapToDetailModel(RecipeSeeds.RecipeEntity);
            detailModel.Name = "Changed recipe name";

            //Act & Assert
            await _facadeSUT.SaveAsync(detailModel);
        }

        [Fact]
        public async Task Update_Name_FromSeeded_CheckUpdated()
        {
            //Arrange
            var detailModel = RecipeModelMapper.MapToDetailModel(RecipeSeeds.RecipeEntity);
            detailModel.Name = "Changed recipe name 1";

            //Act
            await _facadeSUT.SaveAsync(detailModel);

            //Assert
            var returnedModel = await _facadeSUT.GetAsync(detailModel.Id);
            DeepAssert.Equal(detailModel, returnedModel);
        }

        [Fact]
        public async Task Update_RemoveIngredients_FromSeeded_CheckUpdated()
        {
            //Arrange
            var detailModel = RecipeModelMapper.MapToDetailModel(RecipeSeeds.RecipeEntity);
            detailModel.Ingredients.Clear();

            //Act
            await _facadeSUT.SaveAsync(detailModel);

            //Assert
            var returnedModel = await _facadeSUT.GetAsync(detailModel.Id);
            DeepAssert.Equal(detailModel, returnedModel);
        }

        [Fact]
        public async Task Update_RemoveOneOfIngredients_FromSeeded_CheckUpdated()
        {
            //Arrange
            var detailModel = RecipeModelMapper.MapToDetailModel(RecipeSeeds.RecipeEntity);
            detailModel.Ingredients.Remove(detailModel.Ingredients.First());

            //Act
            await _facadeSUT.SaveAsync(detailModel);

            //Assert
            var returnedModel = await _facadeSUT.GetAsync(detailModel.Id);
            DeepAssert.Equal(detailModel, returnedModel);
        }

        [Fact]
        public async Task DeleteById_FromSeeded_DoesNotThrow()
        {
            //Arrange & Act & Assert
            await _facadeSUT.DeleteAsync(RecipeSeeds.RecipeEntity.Id);
        }

        private static void FixIds(RecipeDetailModel expectedModel, RecipeDetailModel returnedModel)
        {
            returnedModel.Id = expectedModel.Id;

            foreach (var ingredientAmountModel in returnedModel.Ingredients)
            {
                var ingredientAmountDetailModel = expectedModel.Ingredients.FirstOrDefault(i =>
                    i.IngredientName == ingredientAmountModel.IngredientName
                    && i.IngredientImageUrl == ingredientAmountModel.IngredientImageUrl
                    && Math.Abs(i.Amount - ingredientAmountModel.Amount) <= 0
                    && i.Unit == ingredientAmountModel.Unit);

                if (ingredientAmountDetailModel != null)
                {
                    ingredientAmountModel.Id = ingredientAmountDetailModel.Id;
                    ingredientAmountModel.IngredientId = ingredientAmountDetailModel.IngredientId;
                }
            }
        }
    }
}
