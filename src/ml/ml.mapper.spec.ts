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
});
