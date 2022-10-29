//
//  WorkflowTableViewCell.swift
//  Wexflow
//
//  Created by Akram El Assas on 27/08/2019.
//  Copyright © 2019 Akram El Assas. All rights reserved.
//

import UIKit

class WorkflowTableViewCell: UITableViewCell {

    @IBOutlet weak var idLabel: UILabel!
    @IBOutlet weak var nameLabel: UILabel!
    @IBOutlet weak var launchTypeLabel: UILabel!
    
    override func awakeFromNib() {
        super.awakeFromNib()
        // Initialization code
    }

    override func setSelected(_ selected: Bool, animated: Bool) {
        super.setSelected(selected, animated: animated)

        // Configure the view for the selected state
    }

}
