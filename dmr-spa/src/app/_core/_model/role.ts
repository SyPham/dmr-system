export interface IRole {
    id: number;
    name: string;
}
export interface IUserRole {
    roleID: number;
    userID: string;
    isLock: boolean;
}
