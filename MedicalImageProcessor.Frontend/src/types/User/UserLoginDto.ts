export interface UserLoginDto {
    /**
     * User email address used for login
     */
    email: string;

    /**
     * Plain-text password for authentication
     */
    password: string;
}