export enum UserErrorType {
    NotFound = 'NOT_FOUND', DatabaseError = 'DATABASE_ERROR',
}

export class UserError extends Error {
    constructor(message: string, public type: UserErrorType) {
        super(message);
        this.name = 'UserError';
    }
}
