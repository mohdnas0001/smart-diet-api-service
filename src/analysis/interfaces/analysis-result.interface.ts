export interface NutrientBreakdown {
  calories?: number;
  protein?: number;
  carbohydrates?: number;
  fat?: number;
  fiber?: number;
  sugar?: number;
  sodium?: number;
  [key: string]: number | undefined;
}

export interface DetectedFood {
  name: string;
  confidence?: number;
  portion?: string;
  calories?: number;
  nutrients?: NutrientBreakdown;
}

export interface NormalizedAnalysisResult {
  detectedFoods: DetectedFood[];
  nutrients: NutrientBreakdown;
  totalCalories: number;
  rawMlResponse: Record<string, unknown>;
}
