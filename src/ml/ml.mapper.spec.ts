import { normalizeMlResponse } from './ml.mapper';

describe('normalizeMlResponse', () => {
  it('normalizes structured food predictions and nutrient totals', () => {
    const result = normalizeMlResponse({
      foods: [
        {
          name: 'Rice',
          confidence: 0.93,
          calories: 200,
          nutrients: {
            protein: 4,
            carbohydrates: 45,
          },
        },
      ],
      totalCalories: 200,
    });

    expect(result.detectedFoods).toEqual([
      {
        name: 'Rice',
        confidence: 0.93,
        portion: undefined,
        calories: 200,
        nutrients: {
          protein: 4,
          carbohydrates: 45,
        },
      },
    ]);
    expect(result.nutrients).toEqual({
      calories: 200,
      protein: 4,
      carbohydrates: 45,
    });
    expect(result.totalCalories).toBe(200);
  });

  it('supports simple string predictions plus top-level nutrition', () => {
    const result = normalizeMlResponse({
      predictions: ['Banana', 'Milk'],
      nutrition: {
        calories: '180',
        fiber: 4,
      },
    });

    expect(result.detectedFoods).toEqual([
      { name: 'Banana' },
      { name: 'Milk' },
    ]);
    expect(result.nutrients).toEqual({
      calories: 180,
      fiber: 4,
    });
    expect(result.totalCalories).toBe(180);
  });

  it('normalizes FastAPI ML service payload shape', () => {
    const result = normalizeMlResponse({
      analysis_id: 'abc-123',
      food_items: [
        {
          name: 'jollof_rice',
          confidence: 0.92,
          portion_grams: 320.5,
          nutrients: {
            calories: 538.4,
            carbohydrates: 87.4,
            protein: 12.2,
            total_fat: 16.6,
            dietary_fiber: 3.1,
          },
        },
      ],
      total_calories: 538.4,
      total_macronutrients: {
        total_calories: 538.4,
        total_protein: 12.2,
        total_carbs: 87.4,
        total_fat: 16.6,
        total_fiber: 3.1,
      },
    });

    expect(result.detectedFoods).toEqual([
      {
        name: 'jollof_rice',
        confidence: 0.92,
        portion: '320.5 g',
        calories: 538.4,
        nutrients: {
          calories: 538.4,
          carbohydrates: 87.4,
          protein: 12.2,
          fat: 16.6,
          fiber: 3.1,
        },
      },
    ]);
    expect(result.nutrients).toEqual({
      calories: 538.4,
      carbohydrates: 87.4,
      protein: 12.2,
      fat: 16.6,
      fiber: 3.1,
      total_calories: 538.4,
      total_protein: 12.2,
      total_fiber: 3.1,
    });
    expect(result.totalCalories).toBe(538.4);
  });
});
