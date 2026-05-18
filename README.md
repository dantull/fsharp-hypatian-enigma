# Hypatian Enigma

A simple web app created using [Fable](http://fable.io/) and [Feliz](https://github.com/Zaid-Ajaj/Feliz) to make a browser interface for solving the Hypatian Enigma hex puzzle.

## Requirements

- [dotnet SDK](https://www.microsoft.com/net/download/core) v8.0 or higher
- [node.js](https://nodejs.org) v20+ LTS

## Editor

To write and edit your code, you can use either VS Code + [Ionide](http://ionide.io/).

## Development

### Setup

This needs to be done only once.

1. `dotnet tool restore` - to install the dotnet tools used in this template
2. `npm i` - to install the npm dependencies

### Scripts

#### Run

Then to start development mode with hot module reloading, run:

```bash
npm start
```

#### Build

To build the application and make ready for production:

```bash
npm run build
```

This command builds the application and puts the generated files into the `/dist` directory (can be overwritten in vite.config.js).

#### Test

To run the tests in watch mode:

```bash
npm test
```
