# AutoSats

[![build](https://github.com/bezysoftware/autosats/actions/workflows/build.yml/badge.svg)](https://github.com/bezysoftware/autosats/actions)
[![version](https://img.shields.io/docker/v/bezysoftware/autosats?sort=semver&cacheSeconds=3600)](https://hub.docker.com/repository/docker/bezysoftware/autosats)

AutoSats is the easiest non-custudial DCA solution you can run on your own node. It lets you periodically buy bitcoin on exchanges according to your defined schedule and automatically withdraw it to your own wallet once it hits a specified threshold.

![](Assets/Screenshot.png)

## What is DCA?

> Dollar-cost averaging (DCA) is an investment strategy in which an investor divides up the total amount to be invested across periodic purchases of a target asset in an effort to reduce the impact of volatility on the overall purchase. The purchases occur regardless of the asset's price and at regular intervals.
[Source](https://www.investopedia.com/terms/d/dollarcostaveraging.asp).

## Supported exchanges

* [Bitfinex](https://bitfinex.com/)
* [Bitstamp](https://bitstamp.net/)
* [Coinbase PRO](https://pro.coinbase.com/)
* [Coinmate](https://coinmate.io/)
* [FTX](https://ftx.com/)
* [Kraken](https://kraken.com/)
* [Poloniex](https://poloniex.com/)
* ...more coming in the future, see below how you can contribute

## Installation

AutoSats can be run in a Docker container. See how to [install docker](https://docs.docker.com/engine/install/).

Latest released version is available on [docker hub](https://hub.docker.com/repository/docker/bezysoftware/autosats/) and can be run in two different modes:

### Bitcoin Core

This uses Bitcoin Core's RPC endpoints.

```bash
docker run \
    -e Wallet__Bitcoind__Auth=<rpc_username:rpc_password> \
    -e Wallet__Bitcoind__Url="http://bitcoin:8332" \
    -p 3311:80 \
    -v autosats:/app_data \
    -d \
    bezysoftware/autosats:latest
```

Let's go over the parameters:
* `-e` sets an enviromental variable, AutoSats needs to know where Bitcoind is running and its RPC credentials
* `-p` exposes the inner port (80) to be available on an external port (3311) - the app will become available on this port (e.g. http://umbrel.local:3311) 
* `-v` mounts a volume on the host to persist data inside the container
* `-d` run the container in detached mode (your console won't be blocked)
* `bezysoftware/autosats:latest` is the name of the image, latest released version
  * You can set a fixed version (`:v0.0.1`)
  * Or you can use absolutely latest version `ghcr.io/bezysoftware/autosats:latest` (built from `main`) which is published to [Github Container Repository](https://github.com/bezysoftware/autosats/pkgs/container/autosats)

### Lightning

There is also an option to use a lightning wallet (e.g. LND) instead of Bitcoind wallet (this will be used in Umbrel):

```bash
mkdir -p /home/umbrel/umbrel/app-data/autosats/data/home
docker run \
    -e Wallet__Type=lightning \
    -e Wallet__Lightning__ConnectionType=lndREST \
    -e Wallet__Lightning__BaseUri="https://lnd:8080" \
    -e Wallet__Lightning__MacaroonFilePath="/lnd/data/chain/bitcoin/mainnet/walletkit.macaroon" \
    -e Wallet__Lightning__CertificatePath="/lnd/tls.cert" \
    -e Application__Password="moneyprintergobrrr" \
    --network umbrel_main_network \
    -p 3311:80 \
    -v /home/umbrel/umbrel/app-data/autosats/data/:/app_data \
    -v /home/umbrel/umbrel/app-data/autosats/data/home:/root \
    -v /home/umbrel/umbrel/lnd:/lnd:ro \
    -d \
    bezysoftware/autosats:latest
```

The long term goal is to make AutoSats available on [Umbrel](https://github.com/getumbrel/umbrel/pull/1039) and similar solutions with one-click install.

## Contributing

AutoSats is in active development and help is certainly welcome. 
Currently it will be mostly needed to cover more exchanges. 
If you want to help to add your favorite exchange:

1) Make sure it's supported by [ExchangeSharp](https://github.com/jjxtra/ExchangeSharp/) (if not then first raise a PR there)
2) Test a schedule (including withdrawal) for your new exchange and create a PR. See this [sample Bitfinex PR](https://github.com/bezysoftware/autosats/pull/3)
3) Attach screenshots from the exchange history screen showing the purchase and withdrawal
