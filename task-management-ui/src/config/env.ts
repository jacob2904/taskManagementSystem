class Config {
  private static instance: Config;

  private constructor() {}

  static getInstance(): Config {
    if (!Config.instance) {
      Config.instance = new Config();
    }
    return Config.instance;
  }

  get apiUrl(): string {
    const baseUrl = import.meta.env.VITE_BASE_URL || "https://localhost:7071";
    return `${baseUrl}/api`;
  }

  get signalRHubUrl(): string {
    const baseUrl = import.meta.env.VITE_BASE_URL || "https://localhost:7071";
    return `${baseUrl}/notificationHub`;
  }

  get isDevelopment(): boolean {
    return import.meta.env.DEV;
  }

  get isProduction(): boolean {
    return import.meta.env.PROD;
  }
}

export const config = Config.getInstance();
