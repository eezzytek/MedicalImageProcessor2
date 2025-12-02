// hooks/useRegister.ts
import { useState } from "react";
import type { UserRegisterDto } from "../../types/User/User";
import type { User } from "../../types/User/User";
import { useAuth } from "../../contexts/AuthContext";

export function useRegister() {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const { login } = useAuth();

    const registerUser = async (dto: UserRegisterDto) => {
        setLoading(true);
        setError(null);

        if (dto.password.length < 6) {
            setError("Пароль має бути мінімум 6 символів");
            setLoading(false);
            return;
        }

        try {
            const response = await fetch(`${import.meta.env.VITE_API_URL}/api/auth/register`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(dto),
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || "Користувач вже існує");
            }

            const data = await response.json();
            const user: User = {
                id: data.userId || "unknown",
                username: dto.username,
                token: data.token,
            };

            login(user);
        } catch (err: any) {
            setError(err.message || "Помилка реєстрації");
        } finally {
            setLoading(false);
        }
    };

    return { registerUser, loading, error };
}
