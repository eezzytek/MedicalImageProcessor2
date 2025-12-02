// hooks/useRegister.ts
import { useState } from "react";
import type { UserRegisterDto } from "../../types/User/UserRegisterDto";
import type { User } from "../../types/User/User";
import { useAuth } from "../../contexts/AuthContext";

export function useRegister() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { login } = useAuth(); // Automatically log in user after register

  const registerUser = async (userDto: UserRegisterDto) => {
    setLoading(true);
    setError(null);

    if (userDto.password !== userDto.confirmPassword) {
      setError("Passwords do not match");
      setLoading(false);
      return;
    }

    try {
      const response = await fetch(`${import.meta.env.VITE_API_URL}/api/Auth/register`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(userDto),
      });

      if (!response.ok) {
        const errorData = await response.text();
        throw new Error(errorData || "Registration failed");
      }

      const data: User = await response.json();
      login(data); // Store user in context & localStorage
    } catch (err: any) {
      setError(err.message || "Something went wrong");
    } finally {
      setLoading(false);
    }
  };

  return { registerUser, loading, error };
}
