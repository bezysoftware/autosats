[![docker build](https://github.com/bezysoftware/autosats/actions/workflows/build.yml/badge.svg)](https://github.com/bezysoftware/autosats/actions)

# AutoSats

AutoSats lets you periodically buy Bitcoin on exchanges and withdraw it to your own wallet. 
It is the simplest non-custudial DCA solution you can run on your own node.

## What is DCA?

> Dollar-cost averaging (DCA) is an investment strategy in which an investor divides up the total amount to be invested across periodic purchases of a target asset in an effort to reduce the impact of volatility on the overall purchase. The purchases occur regardless of the asset's price and at regular intervals.
[Source](https://www.investopedia.com/terms/d/dollarcostaveraging.asp).

## Installation

AutoSats can be run in a Docker container. See how to [install docker](https://docs.docker.com/engine/install/).

Latest images which make their way into the `main` branch are published to [Github Container Repository](https://github.com/bezysoftware/autosats/pkgs/container/autosats) and can be run using

```bash
# Run AutoSats and make it accessible on http://localhost:8080
docker run -p 8080:80 ghcr.io/bezysoftware/autosats:latest
```

Released version will be tagged and published to Dockerhub. 