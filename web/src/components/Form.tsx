import * as React from "react"
import { zodResolver } from "@hookform/resolvers/zod"
import { Controller, useForm } from "react-hook-form"
import { toast } from "sonner"
import * as z from "zod"

import { Button } from "@/components/ui/button"
import {
    Card,
    CardContent,
    CardDescription,
    CardFooter,
    CardHeader,
    CardTitle,
} from "@/components/ui/card"
import {
    Field,
    FieldDescription,
    FieldError,
    FieldGroup,
    FieldLabel,
} from "@/components/ui/field"
import { Input } from "@/components/ui/input"


const loginSchema = z.object({
    email: z.string().email("Invalid email address"),
    password: z.string().min(6, "Password must be at least 6 characters"),
})

type LoginFormValues = z.infer<typeof loginSchema>

export function LoginForm() {
    const {
        control,
        handleSubmit,
        formState: { errors },
    } = useForm<LoginFormValues>({
        resolver: zodResolver(loginSchema),
        defaultValues: {
            email: "",
            password: "",
        },
    })

    React.useEffect(() => {
        console.log("the js is loaded successfully")
    }, [])

    async function onSubmit(data: LoginFormValues) {
        try {
            const params = new URLSearchParams();
            params.append("client_id", import.meta.env.PUBLIC_KEYCLOAK_CLIENT_ID || "wasel-api");
            params.append("grant_type", "password");
            params.append("username", data.email);
            params.append("password", data.password);
            const keycloakUrl = import.meta.env.PUBLIC_KEYCLOAK_URL || (typeof window !== 'undefined' ? window.location.origin + '/auth' : '');
            const realm = import.meta.env.PUBLIC_KEYCLOAK_REALM || "wasel";
            const response = await fetch(`${keycloakUrl}/realms/${realm}/protocol/openid-connect/token`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded",
                },
                body: params
            })
            if (!response.ok) {
                throw new Error("Login failed")
            }
            const result = await response.json()
            console.log(result)
            document.cookie = `access_token=${result.access_token}; path=/; max-age=${result.expires_in}; SameSite=Lax`;
            toast.success("Logged in successfully")
            window.location.href = "/admin/dashboard"
        } catch (error) {
            console.error(error)
            toast.error("Failed to login")
        }
    }

    return (
        <Card className="w-full border-0 shadow-none sm:border sm:shadow-sm bg-transparent sm:bg-card p-4">
            <CardHeader className="px-0 sm:px-6">
                <CardTitle className="text-3xl font-bold">Login</CardTitle>
                <CardDescription>
                    Enter your credentials to login to your account
                </CardDescription>
            </CardHeader>
            <CardContent className="px-0 sm:px-6">
                <form id="login-form" onSubmit={handleSubmit(onSubmit)} className="space-y-6">
                    <FieldGroup>
                        <Controller
                            name="email"
                            control={control}
                            render={({ field, fieldState }) => (
                                <Field aria-invalid={fieldState.invalid}>
                                    <FieldLabel>Email</FieldLabel>
                                    <Input
                                        placeholder="name@example.com"
                                        type="email"
                                        {...field}
                                        aria-invalid={fieldState.invalid}
                                    />
                                    {fieldState.invalid && <FieldError errors={[fieldState.error]} />}
                                </Field>
                            )}
                        />
                        <Controller
                            name="password"
                            control={control}
                            render={({ field, fieldState }) => (
                                <Field aria-invalid={fieldState.invalid}>
                                    <FieldLabel>Password</FieldLabel>
                                    <Input
                                        placeholder="Enter your password"
                                        type="password"
                                        {...field}
                                        aria-invalid={fieldState.invalid}
                                    />
                                    {fieldState.invalid && <FieldError errors={[fieldState.error]} />}
                                </Field>
                            )}
                        />
                    </FieldGroup>

                    {/* w-full makes the button stretch across the form */}
                    <Button type="submit" form="login-form" className="w-full text-lg">
                        Login
                    </Button>
                </form>
            </CardContent>
        </Card>
    )
}