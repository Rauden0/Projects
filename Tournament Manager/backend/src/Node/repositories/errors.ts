export enum NodeErrorType {
    NotFound = 'NOT_FOUND',
    DatabaseError = 'DATABASE_ERROR',
}

export class NodeError extends Error {
    public type: NodeErrorType;
    constructor(message: string, type: NodeErrorType) {
        super(message);
        this.name = 'NodeError';
        this.type = type;
    }
}
