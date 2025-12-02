import { useState } from "react";
import type { UserLoginDto } from "../../types/User/User";
import type { User } from "../../types/User/User";
import { useAuth } from "../../contexts/AuthContext";

export function useLogin() {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const { login } = useAuth();

    const loginUser = async (dto: UserLoginDto) => {
        setLoading(true);
        setError(null);

        try {
            const response = await fetch(`${import.meta.env.VITE_API_URL}/api/auth/login`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(dto),
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || "Неправильний логін або пароль");
            }

            const data = await response.json();
            const user: User = {
                id: data.userId || "unknown",
                username: dto.username,
                token: data.token,
            };

            login(user);
        } catch (err: any) {
            setError(err.message || "Помилка входу");
        } finally {
            setLoading(false);
        }
    };

    return { loginUser, loading, error };
}
