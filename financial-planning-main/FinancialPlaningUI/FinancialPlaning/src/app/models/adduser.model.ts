export interface AddUser{
    id?: string;
    fullName: string;
    username: string;
    email: string;
    phoneNumber: number;
    dob: string;
    address: string;
    roleId : string,
    departmentId: string;
    positionId: string;
    status: number;
    notes: string;
}