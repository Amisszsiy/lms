export interface CheckUserResult {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    tel: string;
    roles: string[];
    expiresAt: string;
}

// Must match RoleNames in the API.
export const RoleNames = {
    admin: 'Admin',
    normalUser: 'NormalUser'
};
