/**
 * Shared client-side auth utilities.
 * JWT lives in localStorage — never in server session cookies.
 */

const Auth = (function () {

  const TOKEN_KEY  = 'at_token';
  const USER_KEY   = 'at_user';
  const EXP_KEY    = 'at_exp';

  // Role claim can appear as either short name or full CLR URI
  const ROLE_KEYS = [
    'role',
    'roles',
    'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
  ];

  function _decode(token) {
    try {
      const b64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
      return JSON.parse(atob(b64));
    } catch { return null; }
  }

  function getToken()   { return localStorage.getItem(TOKEN_KEY);  }
  function getUser()    { try { return JSON.parse(localStorage.getItem(USER_KEY) || '{}'); } catch { return {}; } }
  function isLoggedIn() { return !!getToken(); }

  /** Returns the user's primary role string, or null. */
  function getRole() {
    const token = getToken();
    if (!token) return null;
    const payload = _decode(token);
    if (!payload) return null;

    for (const key of ROLE_KEYS) {
      if (key in payload) {
        const val = payload[key];
        // Could be a string or an array
        return Array.isArray(val) ? val[0] : val;
      }
    }
    // Fallback: scan any key that contains "role"
    for (const k of Object.keys(payload)) {
      if (k.toLowerCase().includes('role')) {
        const val = payload[k];
        return Array.isArray(val) ? val[0] : val;
      }
    }
    return null;
  }

  function isAdmin() { return getRole() === 'Admin'; }

  function isExpired() {
    const token = getToken();
    if (!token) return true;
    const payload = _decode(token);
    if (!payload || !payload.exp) return false;
    return Date.now() / 1000 > payload.exp;
  }

  function logout() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(EXP_KEY);
    window.location.href = '/';
  }

  /**
   * Call at the top of every protected page.
   * @param {'Admin'|'User'|null} requiredRole  null = any authenticated user
   */
  function guard(requiredRole) {
    if (!isLoggedIn() || isExpired()) {
      logout();
      return;
    }
    const role = getRole();
    if (requiredRole === 'Admin' && role !== 'Admin') {
      window.location.replace('/dashboard');
      return;
    }
    if (requiredRole === 'User' && role === 'Admin') {
      window.location.replace('/admin');
      return;
    }
  }

  return { getToken, getUser, getRole, isAdmin, isLoggedIn, isExpired, logout, guard };
})();
