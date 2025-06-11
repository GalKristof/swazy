/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {}, // It's possible we might need to extend colors here if DaisyUI theme extension is not enough.
  },
  plugins: [require("daisyui")],
  daisyui: {
    themes: [
      {
        mytheme: {
          "primary": "#16C979",
          "primary-focus": "#12a362",
          "primary-content": "#ffffff",
        },
      },
      "light",
      "dark",
      "cupcake",
    ],
    darkTheme: "dark",
    base: true,
    styled: true,
    utils: true,
  },
}
