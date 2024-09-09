# Johnny

An experimental pet project about serializing binary data using Source Generators.

<p align="center">:warning: **DISCLAIMER** :warning:</p>

The current state of the code really sucks. The source generator is bad and the whole project isn't organized. If you'd like to help out on this issue, create a pull request.

## Usage (WIP)

Add Johnny to your project using NuGet, after which the `[Johnny]` attribute will be available.

Place the attribute on any struct you'd like to be read from a stream. The struct must be partial.

If all goes well, your struct should now have a static `ReadStruct(BinaryReader reader)` function attached to it.

## Roadmap

Roadmap will be expanded as seen fit.

- [x] Primitive data reading
- [x] Nested structs
- [ ] More complex data types (e.g. collections, dictionaries etc.)

## Issues?

If you encounter any issues, please submit an issue!