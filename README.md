# vcpkg-port-update-alert

> Alerts which vcpkg ports need to be updated

It's great that you can contribute to Microsoft Open-Source projects!
<br/>
One of these projects is [vcpkg](https://github.com/microsoft/vcpkg). You know vcpkg has many ports, and these ports need to be updated when a new release of them is available.

A simple way to contribute to the Microsoft vcpkg, is to find the ports that need to be updated and try to update them. But, finding these ports is hard because vcpkg has more than 1000 ports!

So you can use `vcpkg-port-update-alert` to find the `need-to-be-updated` ports. It search all the ports folder automatically and shows you which ports must be updated.

<p align="center"><img src="https://github.com/ehsan-mohammadi/vcpkg-port-update-alert/blob/master/Images/vcpkg-port-update-alert-gif.gif"/></p>

## Getting started

It's so easy!

- Clone a copy of the repo: `git clone "https://github.com/ehsan-mohammadi/vcpkg-port-update-alert.git"`
- Change to the directory: `cd vcpkg-port-update-alert`
- Install dependencies (If required)
- And run: `dotnet run`

## need-to-be-updated ports

When you running the program, if `need-to-be-update` port found, it will be shown as a yellow text. So, you should to go to the vcpkg repository, click on the port folder and look for a folder that have the same name with the yellow text port. Then try to update it and create a pull request to the `microsoft/vcpkg`.

**Note:** Read the [`vcpkg contributing`](https://github.com/microsoft/vcpkg/blob/master/CONTRIBUTING.md) carefully.

Here is a sample of my PR for vcpkg: [My Pull request to vcpkg: `Update [ecm] port`](https://github.com/microsoft/vcpkg/pull/7457)

## License

This repository is available to anybody free of charge, under the terms of MIT License (See [LICENSE](../master/LICENSE)).