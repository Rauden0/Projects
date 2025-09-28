import 'package:flutter/material.dart';
import 'package:popover/popover.dart';
import 'package:tes/models/campaign.dart';
import 'package:tes/services/campaign_service.dart';
import 'package:tes/services/ioc_container.dart';

class CampaignDateButton extends StatelessWidget {
  final _campaignService = get<CampaignService>();

  final Campaign campaign;

  CampaignDateButton({super.key, required this.campaign});

  @override
  Widget build(BuildContext context) {
    return FilledButton.tonalIcon(
      onPressed: () {
        showPopover(
          context: context,
          bodyBuilder: (context) => Padding(
            padding: const EdgeInsets.all(16.0),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                FilledButton.icon(
                    icon: Icon(Icons.add),
                    onPressed: () {
                      _campaignService.moveTimeForward(
                        campaign: campaign,
                        years: 1,
                      );
                      Navigator.of(context).pop();
                    },
                    label: Text("year")),
                SizedBox(width: 16.0),
                FilledButton.icon(
                    icon: Icon(Icons.add),
                    onPressed: () {
                      _campaignService.moveTimeForward(
                        campaign: campaign,
                        months: 1,
                      );
                      Navigator.of(context).pop();
                    },
                    label: Text("month")),
                SizedBox(width: 16.0),
                FilledButton.icon(
                    icon: Icon(Icons.add),
                    onPressed: () {
                      _campaignService.moveTimeForward(
                        campaign: campaign,
                        days: 1,
                      );
                      Navigator.of(context).pop();
                    },
                    label: Text("day")),
              ],
            ),
          ),
          direction: PopoverDirection.bottom,
          backgroundColor: Theme.of(context).colorScheme.surface,
        );
      },
      icon: Icon(Icons.update),
      label: Text(
        'Move forward',
        style: TextStyle(fontSize: 16.0),
      ),
    );
  }
}
