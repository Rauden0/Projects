async function mapRequestBodyToDatabaseBodyCreate(requestBody: any) {
    return {
        ...requestBody,
        registerDate: new Date(),
        description: null,
        lastLogin: new Date(),
    };
}

export default mapRequestBodyToDatabaseBodyCreate
