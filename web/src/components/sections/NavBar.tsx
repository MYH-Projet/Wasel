import React, { useState, useEffect } from "react";
import { Sheet, SheetContent, SheetTrigger, SheetHeader, SheetTitle, SheetDescription } from "../ui/sheet";
import { Button } from "../ui/button";
import type { GetImageResult } from "astro";
import { Menu } from "lucide-react";



interface NavBarProps {
  logo: GetImageResult
}
export function NavBar(props: NavBarProps) {
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  const links = [
    { name: "Solutions", href: "#solutions" },
    { name: "How It Works", href: "#how-it-works" },
    { name: "Benefits", href: "#benefits" },
    { name: "Drivers", href: "#drivers" },
    { name: "Technology", href: "#technology" },
  ];

  return (
    <header className="fixed top-5 left-1/2 -translate-x-1/2 z-50 mx-auto flex w-[90%] max-w-screen-xl items-center justify-between overflow-visible rounded-2xl border border-border bg-card/80 dark:bg-card/50 backdrop-blur-md p-2 shadow-sm md:w-[80%] lg:w-[75%]">
      {/* Logo */}
      <a
        href="/"
        className="flex h-10 max-h-10 shrink-0 items-center rounded-full px-2 transition-all duration-300 hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-primary/20 focus:ring-offset-2 focus:ring-offset-background"
      >
        <img src={props.logo.src} alt={props.logo.attributes.alt} width={props.logo.attributes.width} height={props.logo.attributes.height} className="h-8 w-auto" />
      </a>

      {!mounted ? (
        <div className="flex min-h-10 items-center lg:min-w-[280px]" aria-hidden />
      ) : (
        <>
          {/* Desktop Navigation */}
          <nav className="hidden lg:flex lg:flex-1 items-center justify-center gap-1" aria-label="Main navigation">
            {links.map((link) => (
              <a
                key={link.name}
                href={link.href}
                className="rounded-lg px-4 py-2 text-base font-medium text-foreground transition-colors duration-300 hover:bg-muted/50"
              >
                {link.name}
              </a>
            ))}
          </nav>

          {/* Mobile Navigation Menu */}
          <div className="flex items-center gap-2 lg:hidden">
            <Sheet>
              <SheetTrigger asChild>
                <button
                  type="button"
                  className="cursor-pointer text-secondary lg:hidden p-2"
                  aria-label="Open menu"
                >
                  <Menu />
                </button>
              </SheetTrigger>
              <SheetContent side="right" className="flex flex-col justify-between border-border bg-card/95 dark:bg-card/90 backdrop-blur-md rounded-tl-2xl rounded-bl-2xl">
                <div>
                  <SheetHeader className="mb-4 text-start">
                    <SheetTitle className="flex items-center justify-start text-secondary">Menu</SheetTitle>
                    <SheetDescription className="sr-only">Main navigation menu</SheetDescription>
                  </SheetHeader>
                  <div className="flex flex-col gap-2">
                    {links.map((link) => (
                      <Button
                        key={link.name}
                        asChild
                        variant="ghost"
                        className="justify-start text-base"
                      >
                        <a href={link.href}>
                          {link.name}
                        </a>
                      </Button>
                    ))}
                    <Button variant="ghost" className="justify-start text-base">
                      Contact Us
                    </Button>
                  </div>
                </div>
              </SheetContent>
            </Sheet>
          </div>

          {/* Desktop Right Side Placeholder */}
          <div className="hidden lg:flex lg:items-center lg:gap-2">
            <Button size="sm" className="rounded-xl">
              Contact Us
            </Button>
          </div>
        </>
      )}
    </header>
  );
}
