import {
  DetectedFood,
  NormalizedAnalysisResult,
  NutrientBreakdown,
} from '../analysis/interfaces/analysis-result.interface';

const NUTRIENT_KEY_ALIASES: Record<string, string> = {
  carbs: 'carbohydrates',
  total_carbs: 'carbohydrates',
  total_fat: 'fat',
  dietary_fiber: 'fiber',
  total_sugars: 'sugar',
};

const toNumber = (value: unknown): number | undefined => {
  if (typeof value === 'number' && Number.isFinite(value)) {
    return value;
  }

  if (typeof value === 'string') {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : undefined;
  }

  return undefined;
};

const toObject = (value: unknown): Record<string, unknown> | undefined => {
  if (typeof value === 'object' && value !== null && !Array.isArray(value)) {
    return value as Record<string, unknown>;
  }

  return undefined;
};

const normalizeNutrients = (value: unknown): NutrientBreakdown => {
  const source = toObject(value);
  if (!source) {
    return {};
  }

  return Object.entries(source).reduce<NutrientBreakdown>(
    (accumulator, [key, entryValue]) => {
      const normalizedValue = toNumber(entryValue);
      if (normalizedValue !== undefined) {
        const normalizedKey = NUTRIENT_KEY_ALIASES[key] ?? key;
        accumulator[normalizedKey] = normalizedValue;
      }

      return accumulator;
    },
    {},
  );
};

const extractFoods = (payload: Record<string, unknown>): DetectedFood[] => {
  const candidates = [
    payload.foods,
    payload.detectedFoods,
    payload.food_items,
    payload.predictions,
    payload.results,
  ];
  const foodList = candidates.find(Array.isArray);
  if (!Array.isArray(foodList)) {
    return [];
  }

  return foodList
    .map<DetectedFood | null>((item) => {
      if (typeof item === 'string') {
        return { name: item };
      }

      const record = toObject(item);
      if (!record) {
        return null;
      }

      const baseNutrients = normalizeNutrients(
        record.nutrients ?? record.nutrition,
      );
      const calories = toNumber(record.calories ?? baseNutrients.calories);
      const nutrients =
        calories !== undefined && baseNutrients.calories === undefined
          ? { ...baseNutrients, calories }
          : baseNutrients;
      const label =
        (typeof record.name === 'string' && record.name) ||
        (typeof record.label === 'string' && record.label) ||
        (typeof record.food === 'string' && record.food) ||
        'Unknown food';

      return {
        name: label,
        confidence: toNumber(record.confidence),
        portion:
          typeof record.portion === 'string'
            ? record.portion
            : toNumber(record.portion_grams) !== undefined
              ? `${toNumber(record.portion_grams)} g`
              : undefined,
        calories,
        nutrients,
      };
    })
    .filter((item): item is DetectedFood => item !== null);
};

const sumNutrients = (foods: DetectedFood[]): NutrientBreakdown => {
  return foods.reduce<NutrientBreakdown>((accumulator, food) => {
    const nutrients = food.nutrients ?? {};
    for (const key of Object.keys(nutrients)) {
      if (key === 'calories') {
        continue;
      }
      const nutrientValue = nutrients[key];
      if (nutrientValue !== undefined) {
        accumulator[key] = (accumulator[key] ?? 0) + nutrientValue;
      }
    }

    const calorieValue = food.calories ?? nutrients.calories;
    if (calorieValue !== undefined) {
      accumulator.calories = (accumulator.calories ?? 0) + calorieValue;
    }

    return accumulator;
  }, {});
};

export const normalizeMlResponse = (
  payload: unknown,
): NormalizedAnalysisResult => {
  const normalizedPayload = toObject(payload) ?? {};
  const detectedFoods = extractFoods(normalizedPayload);
  const payloadNutrients = normalizeNutrients(
    normalizedPayload.nutrients ??
      normalizedPayload.nutrition ??
      normalizedPayload.total_macronutrients,
  );
  const foodsNutrients = sumNutrients(detectedFoods);

  const nutrients: NutrientBreakdown = {
    ...foodsNutrients,
    ...payloadNutrients,
  };

  const totalCalories =
    toNumber(normalizedPayload.totalCalories) ??
    toNumber(normalizedPayload.total_calories) ??
    toNumber(normalizedPayload.calories) ??
    nutrients.calories ??
    0;

  return {
    detectedFoods,
    nutrients,
    totalCalories,
    rawMlResponse: normalizedPayload,
  };
};
