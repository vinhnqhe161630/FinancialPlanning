export interface UserModel{
    id?: string;
    username: string;
    fullName: string;
    email: string;
    phoneNumber: number;
    dob: string;
    address: string;
    department?: string;
    position?: string;
    roleName?: string,
    roleId : string,
    departmentId: string;
    positionId: string;
    status: number;
    notes: string;
}