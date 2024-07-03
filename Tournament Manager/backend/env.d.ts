namespace NodeJS {
    interface ProcessEnv {
        DATABASE_URL?: string;
        PORT?: string;
        NODE_ENV?: 'development' | 'production' | 'test';
    }
}
