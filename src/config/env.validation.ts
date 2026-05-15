const toNumber = (value: string | undefined, fallback: number) => {
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : fallback;
};

const toBoolean = (value: string | undefined, fallback: boolean) => {
  if (value === undefined) {
    return fallback;
  }

  return value.toLowerCase() === 'true';
};

export function validateEnvironment(config: Record<string, unknown>) {
  const environment = config as Record<string, string | undefined>;

  const required = ['JWT_SECRET', 'JWT_REFRESH_SECRET', 'ML_SERVICE_URL'];

  const missing = required.filter((key) => !environment[key]);
  if (missing.length > 0) {
    throw new Error(
      `Missing required environment variables: ${missing.join(', ')}`,
    );
  }

  return {
    PORT: toNumber(environment.PORT, 3000),
    NODE_ENV: environment.NODE_ENV ?? 'development',
    DB_HOST: environment.DB_HOST ?? 'localhost',
    DB_PORT: toNumber(environment.DB_PORT, 5432),
    DB_USERNAME: environment.DB_USERNAME ?? 'postgres',
    DB_PASSWORD: environment.DB_PASSWORD ?? 'postgres',
    DB_DATABASE: environment.DB_DATABASE ?? 'smart_diet',
    DB_SYNCHRONIZE: toBoolean(environment.DB_SYNCHRONIZE, true),
    JWT_SECRET: environment.JWT_SECRET,
    JWT_EXPIRES_IN: environment.JWT_EXPIRES_IN ?? '15m',
    JWT_REFRESH_SECRET: environment.JWT_REFRESH_SECRET,
    JWT_REFRESH_EXPIRES_IN: environment.JWT_REFRESH_EXPIRES_IN ?? '7d',
    BCRYPT_SALT_ROUNDS: toNumber(environment.BCRYPT_SALT_ROUNDS, 10),
    ML_SERVICE_URL: environment.ML_SERVICE_URL,
    ML_ANALYSIS_ENDPOINT: environment.ML_ANALYSIS_ENDPOINT ?? '/api/predict',
    ML_ANALYSIS_FILE_FIELD: environment.ML_ANALYSIS_FILE_FIELD ?? 'file',
    ML_SERVICE_TIMEOUT_MS: toNumber(environment.ML_SERVICE_TIMEOUT_MS, 15000),
  };
}
