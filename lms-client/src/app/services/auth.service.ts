import { HttpClient } from "@angular/common/http";
import { computed, Injectable, signal } from "@angular/core";
import { catchError, map, Observable, of, tap } from "rxjs";
import { environment } from "../../environments/environment";
import { LoginRequest } from "../models/loginRequest";
import { RegisterRequest } from "../models/registerRequest";
import { CheckUserResult, RoleNames } from "../models/checkUserResult";


@Injectable({providedIn: 'root'})
export class AuthService {
    private readonly baseUrl = environment.apiUrl;

    private isAuthenticated = signal<boolean | null>(null);
    isAuthenticated$ = this.isAuthenticated.asReadonly();

    private currentUser = signal<CheckUserResult | null>(null);
    currentUser$ = this.currentUser.asReadonly();

    userId$ = computed(() => {
        const user = this.currentUser();
        if (user === null) {
            return null;
        }
        return user.id;
    });

    // Used to decide which menu is shown and which routes can be opened.
    isAdmin$ = computed(() => {
        const user = this.currentUser();
        if (user === null) {
            return false;
        }
        return user.roles.includes(RoleNames.admin);
    });

    constructor(private http: HttpClient){}

    register(credential: RegisterRequest){
        return this.http.post(`${this.baseUrl}/api/auth/register`, credential);
    }

    checkAuth(): Observable<boolean> {
        return this.http.get<CheckUserResult>(`${this.baseUrl}/api/auth/check-user`).pipe(
            map((res: CheckUserResult) => {
                this.isAuthenticated.set(true);
                this.currentUser.set(res);
                return true;
            }),
            catchError(() => {
                this.isAuthenticated.set(false);
                this.currentUser.set(null);
                return of(false);
            })
        );
    }

    login(credential: LoginRequest){
        return this.http.post(`${this.baseUrl}/api/auth/login`, credential).pipe(
            tap(() => this.isAuthenticated.set(true))
        );
    }

    logout(){
        return this.http.post(`${this.baseUrl}/api/auth/logout`, {}).pipe(
            tap(() => {
                this.isAuthenticated.set(false);
                this.currentUser.set(null);
            })
        );
    }
}
