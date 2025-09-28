import 'package:flutter/material.dart';
import 'package:tes/models/campaign.dart';
import 'package:tes/pages/campaign_detail_page.dart';
import 'package:tes/widgets/campaign/campaign_options.dart';

class CampaignTile extends StatelessWidget {
  final Campaign campaign;
  final VoidCallback onChangeArchived;
  final VoidCallback onDelete;

  CampaignTile(
      {super.key,
      required this.campaign,
      required this.onChangeArchived,
      required this.onDelete});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: FilledButton.tonalIcon(
        iconAlignment: IconAlignment.end,
        icon: CampaignOptions(
          campaign: campaign,
          onChangeArchived: onChangeArchived,
          onDelete: onDelete,
        ),
        onPressed: () {
          Navigator.of(context).push(
            MaterialPageRoute(
              builder: (context) => CampaignDetailPage(
                campaignId: campaign.id,
              ),
            ),
          );
        },
        label: Padding(
          padding: const EdgeInsets.symmetric(vertical: 16.0),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                campaign.name,
                style: TextStyle(fontSize: 16.0),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
