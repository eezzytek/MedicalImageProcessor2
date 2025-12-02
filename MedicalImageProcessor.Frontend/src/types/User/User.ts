// types/User/User.ts
export interface User {
    id: string;
    username: string;
    token: string;
}

export interface UserLoginDto {
    username: string;
    password: string;
}

export interface UserRegisterDto {
    username: string;
    password: string;
}
