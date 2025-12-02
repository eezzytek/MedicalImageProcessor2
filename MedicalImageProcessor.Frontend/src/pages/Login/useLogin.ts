import { useState } from "react";
import type { UserLoginDto } from "../../types/User/UserLoginDto";
import type { User } from "../../types/User/User"
import { useAuth } from "../../contexts/AuthContext";

export function useLogin() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { login } = useAuth();

  const loginUser = async (userDto: UserLoginDto) => {
    setLoading(true);
    setError(null);

    try {
      const response = await fetch(`${import.meta.env.VITE_API_URL}/api/Auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(userDto),
      });

      if (!response.ok) {
        const errorData = await response.text();
        throw new Error(errorData || "Login failed");
      }

      const data: User = await response.json();
      login(data); // store user in context & localStorage
    } catch (err: any) {
      setError(err.message || "Something went wrong");
    } finally {
      setLoading(false);
    }
  };

  return { loginUser, loading, error };
}
