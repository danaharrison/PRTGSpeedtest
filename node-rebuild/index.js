import fetch from 'node-fetch';
import * as dotenv from 'dotenv';
import { execSync } from 'child_process'; 
dotenv.config();

const PIHOLE_LIST=process.env.PIHOLE_LIST.split('|');
const PIHOLE_API_KEY=process.env.PIHOLE_API_KEY;

const getInternetSpeed = function(internet_callback) {
    return execSync('fast --upload --json').toString();
}

async function queryPiHoles(piHoles) {
    let dnsQueries = 0;
    let adsBlocked = 0;

    for(let piHole of piHoles) {
        const response = await fetch(`http://${piHole}/admin/api.php?summaryRaw&auth=${PIHOLE_API_KEY}`);
        const data = await response.json();

        dnsQueries += data.dns_queries_today;
        adsBlocked += data.ads_blocked_today;
    }

    return `${dnsQueries}|${adsBlocked}`;
}

async function main() {
    let piHoleResult = await queryPiHoles(PIHOLE_LIST);
    let speedTestResult = JSON.parse(getInternetSpeed());

    let prtgResult = {
        prtg: {
            result: [
                {
                    channel: "Download",
                    value: speedTestResult.downloadSpeed,
                    unit: "custom",
                    customunit: "Mbps"
                },
                {
                    channel: "Upload",
                    value: speedTestResult.uploadSpeed,
                    unit: "custom",
                    customunit: "Mbps"
                },
                {
                    channel: "Latency",
                    value: speedTestResult.latency,
                    unit: "custom",
                    customUnit: "ms"
                },
                {
                    channel: "PiHole Queries", 
                    value: `${piHoleResult.split('|')[0]}`,
                    unit: null,
                    customunit: null
                },
                {
                    channel: "Ads Blocked",
                    value: `${piHoleResult.split('|')[1]}`,
                    unit: null,
                    customunit: null
                }
            ],
            text: ""
        }
    }

    console.log(JSON.stringify(prtgResult))
}

main();