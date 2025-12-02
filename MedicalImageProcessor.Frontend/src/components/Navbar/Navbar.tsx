import { useAuth } from "../../contexts/AuthContext";
import "./navbar.css";

export const Navbar = () => {
  const { user, logout } = useAuth();

  return (
    <nav className="navbar">
      <h1 className="navbar__brand">MedicalImageProcessor</h1>
      <div className="navbar__links">
        {user ? (
          <>
            <span className="navbar__user">Hello, {user.username}</span>
            <button className="navbar__button" onClick={logout}>
              Logout
            </button>
          </>
        ) : (
          <>
            <a href="/login" className="navbar__link">
              Login
            </a>
            <a href="/register" className="navbar__link">
              Register
            </a>
          </>
        )}
      </div>
    </nav>
  );
};
